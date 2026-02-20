using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        return await _userService.GetCurrentUserAsync(userId.Value, HttpContext.RequestAborted);
    }

    private Guid? GetUserId()
    {
        var uid = User.FindFirstValue("uid");
        return Guid.TryParse(uid, out var id) ? id : null;
    }
}
