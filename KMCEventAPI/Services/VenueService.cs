using KMCEventAPI.Data;
using KMCEventAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace KMCEventAPI.Services;

public class VenueService
{
    private readonly KmcDbContext _db;

    public VenueService(KmcDbContext db)
    {
        _db = db;
    }

    public async Task<List<VenueDto>> GetAllVenuesAsync()
    {
        return await _db.Venues
            .AsNoTracking()
            .OrderBy(x => x.VenueName)
            .Select(x => new VenueDto
            {
                VenueID = x.VenueID,
                VenueName = x.VenueName,
                Address = x.Address,
                City = x.City,
                Capacity = x.Capacity,
                Description = x.Description
            })
            .ToListAsync();
    }

    public async Task<VenueDto?> GetVenueByIdAsync(int venueId)
    {
        return await _db.Venues
            .AsNoTracking()
            .Where(x => x.VenueID == venueId)
            .Select(x => new VenueDto
            {
                VenueID = x.VenueID,
                VenueName = x.VenueName,
                Address = x.Address,
                City = x.City,
                Capacity = x.Capacity,
                Description = x.Description
            })
            .FirstOrDefaultAsync();
    }
}
