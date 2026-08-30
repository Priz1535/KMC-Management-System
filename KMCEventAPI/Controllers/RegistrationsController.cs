using System.Security.Claims;
using KMCEventAPI.DTOs;
using KMCEventAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMCEventAPI.Controllers;

[ApiController]
[Route("api/registrations")]
public class RegistrationsController : ControllerBase
{
    private readonly RegistrationService _registrationService;

    public RegistrationsController(RegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpPost]
    public async Task<ActionResult<RegistrationDto>> Register(RegistrationRequestDto request)
    {
        try
        {
            var result = await _registrationService.RegisterForEventAsync(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("event/{eventId:int}")]
    public async Task<ActionResult<List<RegistrationDto>>> GetForEvent(int eventId)
    {
        try
        {
            return Ok(await _registrationService.GetRegistrationsForEventAsync(eventId, GetOrganizerId()));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    private int GetOrganizerId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var organizerId)
            ? organizerId
            : throw new UnauthorizedAccessException("Organizer identity is missing from the token.");
    }
}
