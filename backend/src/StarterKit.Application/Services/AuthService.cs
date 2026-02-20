using StarterKit.Application.Dtos;
using StarterKit.Application.Interfaces;
using StarterKit.Domain.Entities;
using StarterKit.Domain.Interfaces.Repositories;
using StarterKit.Domain.Interfaces.Security;
using StarterKit.Domain.Interfaces.Services;

namespace StarterKit.Application.Services;

public sealed class AuthService : IAuthService
{
    private const int MinPasswordLength = 8;

    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ITokenHashingService _tokenHashingService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ITokenHashingService tokenHashingService,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _tokenHashingService = tokenHashingService;
        _unitOfWork = unitOfWork;
    }

    // TODO: Improve responces so that we can remove throw new XYZ
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateCredentials(request.Email, request.Password);

        if (await _users.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var hashed = _passwordHasher.Hash(request.Password);
        var user = User.CreateNew(request.Email, hashed);
        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        ValidateCredentials(request.Email, request.Password);

        var user = await _users.GetByEmailAsync(request.Email, cancellationToken)
                   ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new ArgumentException("Refresh token is required.");

        var hash = _tokenHashingService.Hash(request.RefreshToken);
        var stored = await _refreshTokens.GetByHashAsync(hash, cancellationToken);
        if (stored is null || !stored.IsActive)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        stored.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _users.GetByIdAsync(stored.UserId, cancellationToken)
                   ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task LogoutAsync(RefreshRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken)) return;

        var hash = _tokenHashingService.Hash(request.RefreshToken);
        var stored = await _refreshTokens.GetByHashAsync(hash, cancellationToken);
        if (stored is null) return;

        stored.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _tokenService.CreateAccessToken(user);
        var refresh = _tokenService.CreateRefreshToken();

        var refreshEntity = new RefreshToken(user.Id, refresh.Hash, refresh.ExpiresUtc);
        await _refreshTokens.AddAsync(refreshEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            accessToken,
            refresh.Token,
            "Bearer",
            _tokenService.AccessTokenExpiresSeconds,
            refresh.ExpiresUtc);
    }

    // TODO: seperate the validation, we do not want
    // the login to tell how many characters the password must have.
    private static void ValidateCredentials(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Email and password are required.");

        if (password.Length < MinPasswordLength)
            throw new ArgumentException("Password must be at least 8 characters.");
    }
}
