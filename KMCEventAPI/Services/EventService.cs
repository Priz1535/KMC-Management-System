using KMCEventAPI.Data;
using KMCEventAPI.DTOs;
using KMCEventAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KMCEventAPI.Services;

public class EventService
{
    private readonly KmcDbContext _db;

    public EventService(KmcDbContext db)
    {
        _db = db;
    }

    public async Task<List<EventDto>> GetAllEventsAsync(DateTime? date = null, string? type = null)
    {
        var query = _db.Events
            .AsNoTracking()
            .Include(x => x.Organizer)
            .Include(x => x.Venue)
            .Include(x => x.Registrations)
            .AsQueryable();

        if (date.HasValue)
        {
            var selectedDate = date.Value.Date;
            query = query.Where(x => x.EventDate.Date == selectedDate);
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var eventType = type.Trim();
            query = query.Where(x => x.EventType.Contains(eventType));
        }

        var events = await query
            .OrderBy(x => x.EventDate)
            .ThenBy(x => x.EventTime)
            .ToListAsync();

        return events.Select(ToDto).ToList();
    }

    public Task<List<EventDto>> SearchEventsAsync(DateTime? date, string? type)
    {
        return GetAllEventsAsync(date, type);
    }

    public async Task<EventDto?> GetEventByIdAsync(int eventId)
    {
        var item = await _db.Events
            .AsNoTracking()
            .Include(x => x.Organizer)
            .Include(x => x.Venue)
            .Include(x => x.Registrations)
            .FirstOrDefaultAsync(x => x.EventID == eventId);

        return item is null ? null : ToDto(item);
    }

    public async Task<List<EventDto>> GetOrganizerEventsAsync(int organizerId)
    {
        var events = await _db.Events
            .AsNoTracking()
            .Include(x => x.Organizer)
            .Include(x => x.Venue)
            .Include(x => x.Registrations)
            .Where(x => x.OrganizerID == organizerId)
            .OrderBy(x => x.EventDate)
            .ThenBy(x => x.EventTime)
            .ToListAsync();

        return events.Select(ToDto).ToList();
    }

    public async Task<EventDto> CreateEventAsync(EventUpsertDto request, int organizerId)
    {
        await ValidateVenueAndCapacityAsync(request.VenueID, request.Capacity);
        ValidateEventDate(request.EventDate);

        var item = new Event
        {
            EventName = request.EventName.Trim(),
            Description = request.Description.Trim(),
            EventDate = request.EventDate.Date,
            EventTime = request.EventTime,
            EventType = request.EventType.Trim(),
            Capacity = request.Capacity,
            OrganizerID = organizerId,
            VenueID = request.VenueID,
            CreatedAt = DateTime.UtcNow
        };

        _db.Events.Add(item);
        await _db.SaveChangesAsync();

        return (await GetEventByIdAsync(item.EventID))!;
    }

    public async Task<EventDto> UpdateEventAsync(int eventId, EventUpsertDto request, int organizerId)
    {
        var item = await _db.Events.FirstOrDefaultAsync(x => x.EventID == eventId)
            ?? throw new KeyNotFoundException("Event not found.");

        CheckEventOwnership(item, organizerId);
        await ValidateVenueAndCapacityAsync(request.VenueID, request.Capacity);
        ValidateEventDate(request.EventDate);

        var registeredCount = await _db.Registrations.CountAsync(x => x.EventID == eventId);
        if (request.Capacity < registeredCount)
        {
            throw new InvalidOperationException(
                $"Capacity cannot be lower than the current registration count ({registeredCount}).");
        }

        item.EventName = request.EventName.Trim();
        item.Description = request.Description.Trim();
        item.EventDate = request.EventDate.Date;
        item.EventTime = request.EventTime;
        item.EventType = request.EventType.Trim();
        item.Capacity = request.Capacity;
        item.VenueID = request.VenueID;

        await _db.SaveChangesAsync();

        return (await GetEventByIdAsync(item.EventID))!;
    }

    public async Task DeleteEventAsync(int eventId, int organizerId)
    {
        var item = await _db.Events.FirstOrDefaultAsync(x => x.EventID == eventId)
            ?? throw new KeyNotFoundException("Event not found.");

        CheckEventOwnership(item, organizerId);
        _db.Events.Remove(item);
        await _db.SaveChangesAsync();
    }

    public void CheckEventOwnership(Event item, int organizerId)
    {
        if (item.OrganizerID != organizerId)
        {
            throw new UnauthorizedAccessException("You can only modify events that you created.");
        }
    }

    private async Task ValidateVenueAndCapacityAsync(int venueId, int eventCapacity)
    {
        var venue = await _db.Venues.AsNoTracking().FirstOrDefaultAsync(x => x.VenueID == venueId)
            ?? throw new InvalidOperationException("Selected venue does not exist.");

        if (eventCapacity > venue.Capacity)
        {
            throw new InvalidOperationException(
                $"Event capacity cannot exceed the selected venue capacity of {venue.Capacity}.");
        }
    }

    private static void ValidateEventDate(DateTime eventDate)
    {
        if (eventDate.Date < DateTime.Today)
        {
            throw new InvalidOperationException("Event date cannot be in the past.");
        }
    }

    private static EventDto ToDto(Event item) => new()
    {
        EventID = item.EventID,
        EventName = item.EventName,
        Description = item.Description,
        EventDate = item.EventDate,
        EventTime = item.EventTime,
        EventType = item.EventType,
        Capacity = item.Capacity,
        RegisteredCount = item.Registrations?.Count ?? 0,
        CreatedAt = item.CreatedAt,
        OrganizerID = item.OrganizerID,
        OrganizerName = item.Organizer?.OrganizerName ?? string.Empty,
        OrganizationName = item.Organizer?.OrganizationName ?? string.Empty,
        VenueID = item.VenueID,
        VenueName = item.Venue?.VenueName ?? string.Empty,
        VenueAddress = item.Venue?.Address ?? string.Empty,
        VenueCity = item.Venue?.City ?? string.Empty
    };
}
