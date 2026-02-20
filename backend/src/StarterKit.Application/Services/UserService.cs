using StarterKit.Application.Dtos;
using StarterKit.Application.Interfaces;
using StarterKit.Domain.Interfaces.Repositories;

namespace StarterKit.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;

    public UserService(IUserRepository users)
    {
        _users = users;
    }

    public async Task<MeResponseDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken)
                   ?? throw new UnauthorizedAccessException();

        return new MeResponseDto(user.Id, user.Email, user.CreatedUtc);
    }
}
