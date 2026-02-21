using Microsoft.AspNetCore.Mvc;
using StarterKit.Api.Errors;
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

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request, HttpContext.RequestAborted);
        if (!result.IsSuccess || result.Value is null)
            return ApplicationErrorMapper.Map<AuthResponseDto>(this, result.Error);

        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request, HttpContext.RequestAborted);
        if (!result.IsSuccess || result.Value is null)
            return ApplicationErrorMapper.Map<AuthResponseDto>(this, result.Error);

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh([FromBody] RefreshRequestDto request)
    {
        var result = await _authService.RefreshAsync(request, HttpContext.RequestAborted);
        if (!result.IsSuccess || result.Value is null)
            return ApplicationErrorMapper.Map<AuthResponseDto>(this, result.Error);

        return Ok(result.Value);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto request)
    {
        var result = await _authService.LogoutAsync(request, HttpContext.RequestAborted);
        if (!result.IsSuccess)
            return ApplicationErrorMapper.Map(this, result.Error);

        return NoContent();
    }
}
