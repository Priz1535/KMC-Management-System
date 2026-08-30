using KMCEventAPI.DTOs;
using KMCEventAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace KMCEventAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterOrganizerDto request)
    {
        var result = await _authService.RegisterOrganizerAsync(request);
        if (!result.Success)
        {
            return Conflict(new { message = result.Message });
        }

        return StatusCode(StatusCodes.Status201Created, new
        {
            message = result.Message,
            organizer = result.Organizer
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        var result = await _authService.LoginOrganizerAsync(request);
        if (result is null)
        {
            return Unauthorized(new { message = "Invalid email address or password." });
        }

        return Ok(result);
    }
}
