using StarterKit.Application.Dtos;
using StarterKit.Application.Results;

namespace StarterKit.Application.Interfaces;

public interface IUserService
{
    Task<Result<MeResponseDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
