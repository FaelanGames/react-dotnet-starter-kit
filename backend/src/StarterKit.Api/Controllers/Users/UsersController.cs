using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarterKit.Api.Errors;
using StarterKit.Application.Dtos;
using StarterKit.Application.Interfaces;

namespace StarterKit.Api.Features.Users;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponseDto>> Me()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var result = await _userService.GetCurrentUserAsync(userId.Value, HttpContext.RequestAborted);
        if (!result.IsSuccess || result.Value is null)
            return ApplicationErrorMapper.Map<MeResponseDto>(this, result.Error);

        return result.Value;
    }

    private Guid? GetUserId()
    {
        var uid = User.FindFirstValue("uid");
        return Guid.TryParse(uid, out var id) ? id : null;
    }

}
