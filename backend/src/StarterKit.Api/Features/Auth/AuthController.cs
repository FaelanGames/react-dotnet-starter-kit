using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StarterKit.Api.Auth;
using StarterKit.Api.Data;
using StarterKit.Api.Features.Users;

namespace StarterKit.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    private readonly JwtOptions _jwt;
    private const int SqliteConstraintErrorCode = 19;

    public AuthController(AppDbContext db, TokenService tokens, IOptions<JwtOptions> jwt)
    {
        _db = db;
        _tokens = tokens;
        _jwt = jwt.Value;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest req)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email and password are required.");

        if (req.Password.Length < 8)
            return BadRequest("Password must be at least 8 characters.");

        bool exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists) return Conflict("Email is already registered.");

        var user = new User
        {
            Email = email,
            PasswordHash = PasswordHasher.Hash(req.Password)
        };

        _db.Users.Add(user);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            return Conflict("Email is already registered.");
        }

        var response = await IssueTokensAsync(user);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest req)
    {
        var email = (req.Email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email and password are required.");

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null) return Unauthorized("Invalid credentials.");

        if (!PasswordHasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        var response = await IssueTokensAsync(user);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            return BadRequest("Refresh token is required.");

        var hash = TokenService.HashRefreshToken(req.RefreshToken);
        var stored = await _db.RefreshTokens
            .Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.TokenHash == hash);

        if (stored is null || !stored.IsActive || stored.User is null)
            return Unauthorized("Invalid refresh token.");

        stored.RevokedUtc = DateTime.UtcNow;

        var response = await IssueTokensAsync(stored.User);
        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            return BadRequest("Refresh token is required.");

        var hash = TokenService.HashRefreshToken(req.RefreshToken);
        var stored = await _db.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.TokenHash == hash);

        if (stored is null)
            return NoContent();

        if (stored.RevokedUtc is null)
            stored.RevokedUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<AuthResponse> IssueTokensAsync(User user)
    {
        var accessToken = _tokens.CreateToken(user);
        var refresh = _tokens.CreateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refresh.Hash,
            ExpiresUtc = refresh.ExpiresUtc
        });

        await _db.SaveChangesAsync();

        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: refresh.Token,
            TokenType: "Bearer",
            ExpiresInSeconds: _jwt.ExpiresMinutes * 60,
            RefreshTokenExpiresUtc: refresh.ExpiresUtc
        );
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        if (ex.InnerException is SqliteException sqliteEx)
        {
            return sqliteEx.SqliteErrorCode == SqliteConstraintErrorCode;
        }

        return false;
    }
}
