using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarterKit.Api.Data;

namespace StarterKit.Api.Features.Users;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var user = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId.Value);

        if (user is null) return Unauthorized();

        return Ok(new MeResponse(user.Id, user.Email, user.CreatedUtc));
    }

    private Guid? GetUserId()
    {
        var uid = User.FindFirstValue("uid");
        return Guid.TryParse(uid, out var id) ? id : null;
    }
}
