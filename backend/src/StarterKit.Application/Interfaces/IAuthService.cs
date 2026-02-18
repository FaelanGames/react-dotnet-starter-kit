using StarterKit.Application.Dtos;

namespace StarterKit.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken = default);
    Task LogoutAsync(RefreshRequestDto request, CancellationToken cancellationToken = default);
}
