using StarterKit.Application.Dtos;
using StarterKit.Application.Interfaces;
using StarterKit.Application.Results;
using StarterKit.Domain.Interfaces.Repositories;

namespace StarterKit.Application.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;

    public UserService(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result<MeResponseDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<MeResponseDto>.Failure(new AppError(ErrorCode.UserNotFound, "User not found."));
        }

        return Result<MeResponseDto>.Success(new MeResponseDto(user.Id, user.Email, user.CreatedUtc));
    }
}
