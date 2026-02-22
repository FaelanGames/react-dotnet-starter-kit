using StarterKit.Application.Dtos;
using StarterKit.Application.Interfaces;
using StarterKit.Application.Results;
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

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateCredentials(request.Email, request.Password);
        if (validationError is not null)
            return Result<AuthResponseDto>.Failure(validationError);

        if (await _users.EmailExistsAsync(request.Email, cancellationToken))
        {
            return Result<AuthResponseDto>.Failure(new AppError(
                ErrorCode.EmailAlreadyRegistered,
                "Email is already registered."));
        }

        var hashed = _passwordHasher.Hash(request.Password);
        var user = User.CreateNew(request.Email, hashed);
        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateCredentials(request.Email, request.Password);
        if (validationError is not null)
            return Result<AuthResponseDto>.Failure(validationError);

        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result<AuthResponseDto>.Failure(new AppError(
                ErrorCode.InvalidCredentials,
                "Invalid credentials."));
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Failure(new AppError(
                ErrorCode.InvalidCredentials,
                "Invalid credentials."));
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponseDto>> RefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Result<AuthResponseDto>.Failure(new AppError(
                ErrorCode.ValidationFailed,
                "Refresh token is required."));
        }

        var hash = _tokenHashingService.Hash(request.RefreshToken);
        var stored = await _refreshTokens.GetByHashAsync(hash, cancellationToken);
        if (stored is null || !stored.IsActive)
        {
            return Result<AuthResponseDto>.Failure(new AppError(
                ErrorCode.InvalidRefreshToken,
                "Invalid refresh token."));
        }

        stored.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _users.GetByIdAsync(stored.UserId, cancellationToken);
        if (user is null)
        {
            return Result<AuthResponseDto>.Failure(new AppError(
                ErrorCode.InvalidRefreshToken,
                "Invalid refresh token."));
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<Result> LogoutAsync(RefreshRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result.Success();

        var hash = _tokenHashingService.Hash(request.RefreshToken);
        var stored = await _refreshTokens.GetByHashAsync(hash, cancellationToken);
        if (stored is null)
            return Result.Success();

        stored.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<Result<AuthResponseDto>> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = _tokenService.CreateAccessToken(user);
        var refresh = _tokenService.CreateRefreshToken();

        var refreshEntity = new RefreshToken(user.Id, refresh.Hash, refresh.ExpiresUtc);
        await _refreshTokens.AddAsync(refreshEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            accessToken,
            refresh.Token,
            "Bearer",
            _tokenService.AccessTokenExpiresSeconds,
            refresh.ExpiresUtc));
    }

    private static AppError? ValidateCredentials(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new AppError(ErrorCode.ValidationFailed, "Email and password are required.");
        }

        if (password.Length < MinPasswordLength)
        {
            return new AppError(ErrorCode.ValidationFailed, "Password must be at least 8 characters.");
        }

        return null;
    }
}
