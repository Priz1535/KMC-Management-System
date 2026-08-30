using KMCEventAPI.DTOs;
using KMCEventAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace KMCEventAPI.Controllers;

[ApiController]
[Route("api/venues")]
public class VenuesController : ControllerBase
{
    private readonly VenueService _venueService;

    public VenuesController(VenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpGet]
    public async Task<ActionResult<List<VenueDto>>> GetAll()
    {
        return Ok(await _venueService.GetAllVenuesAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VenueDto>> GetById(int id)
    {
        var result = await _venueService.GetVenueByIdAsync(id);
        return result is null ? NotFound(new { message = "Venue not found." }) : Ok(result);
    }
}
