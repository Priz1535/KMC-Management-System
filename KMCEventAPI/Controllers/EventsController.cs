using System.Security.Claims;
using KMCEventAPI.DTOs;
using KMCEventAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMCEventAPI.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly EventService _eventService;

    public EventsController(EventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<ActionResult<List<EventDto>>> GetAll([FromQuery] DateTime? date, [FromQuery] string? type)
    {
        return Ok(await _eventService.SearchEventsAsync(date, type));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var result = await _eventService.GetEventByIdAsync(id);
        return result is null ? NotFound(new { message = "Event not found." }) : Ok(result);
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<List<EventDto>>> GetMine()
    {
        return Ok(await _eventService.GetOrganizerEventsAsync(GetOrganizerId()));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<EventDto>> Create(EventUpsertDto request)
    {
        try
        {
            var created = await _eventService.CreateEventAsync(request, GetOrganizerId());
            return CreatedAtAction(nameof(GetById), new { id = created.EventID }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<EventDto>> Update(int id, EventUpsertDto request)
    {
        try
        {
            return Ok(await _eventService.UpdateEventAsync(id, request, GetOrganizerId()));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _eventService.DeleteEventAsync(id, GetOrganizerId());
            return NoContent();
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
