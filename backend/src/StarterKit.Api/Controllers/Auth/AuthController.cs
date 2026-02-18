using Microsoft.AspNetCore.Mvc;
using StarterKit.Application.Dtos;
using StarterKit.Application.Interfaces;

namespace StarterKit.Api.Features.Auth;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // TODO: Improve reponses
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        var response = await _authService.RegisterAsync(request, HttpContext.RequestAborted);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request, HttpContext.RequestAborted);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshRequestDto request)
    {
        var response = await _authService.RefreshAsync(request, HttpContext.RequestAborted);
        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto request)
    {
        await _authService.LogoutAsync(request, HttpContext.RequestAborted);
        return NoContent();
    }
}
