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

        return Ok(ToAuthResponse(user));
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

        return Ok(ToAuthResponse(user));
    }

    private AuthResponse ToAuthResponse(User user)
    {
        var token = _tokens.CreateToken(user);
        return new AuthResponse(
            AccessToken: token,
            TokenType: "Bearer",
            ExpiresInSeconds: _jwt.ExpiresMinutes * 60
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
