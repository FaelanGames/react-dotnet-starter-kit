using StarterKit.Application.Dtos;
using StarterKit.Application.Results;

namespace StarterKit.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> RefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(RefreshRequestDto request, CancellationToken cancellationToken = default);
}
