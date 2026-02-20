using StarterKit.Application.Dtos;

namespace StarterKit.Application.Interfaces;

public interface IUserService
{
    Task<MeResponseDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
