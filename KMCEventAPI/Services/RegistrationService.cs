using KMCEventAPI.Data;
using KMCEventAPI.DTOs;
using KMCEventAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KMCEventAPI.Services;

public class RegistrationService
{
    private readonly KmcDbContext _db;

    public RegistrationService(KmcDbContext db)
    {
        _db = db;
    }

    public async Task<RegistrationDto> RegisterForEventAsync(RegistrationRequestDto request)
    {
        var eventItem = await _db.Events
            .Include(x => x.Registrations)
            .FirstOrDefaultAsync(x => x.EventID == request.EventID)
            ?? throw new KeyNotFoundException("The selected event could not be found.");

        if (eventItem.EventDate.Date < DateTime.Today)
        {
            throw new InvalidOperationException("Registration is closed because this event has already passed.");
        }

        if (eventItem.Registrations.Count >= eventItem.Capacity)
        {
            throw new InvalidOperationException("This event has reached its registration capacity.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var publicUser = await _db.PublicUsers.FirstOrDefaultAsync(x => x.Email == normalizedEmail);

        if (publicUser is null)
        {
            publicUser = new PublicUser
            {
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                PhoneNumber = request.PhoneNumber.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _db.PublicUsers.Add(publicUser);
            await _db.SaveChangesAsync();
        }
        else
        {
            publicUser.FullName = request.FullName.Trim();
            publicUser.PhoneNumber = request.PhoneNumber.Trim();
        }

        var alreadyRegistered = await CheckRegistrationAsync(publicUser.PublicUserID, request.EventID);

        if (alreadyRegistered)
        {
            throw new InvalidOperationException("This email address is already registered for the selected event.");
        }

        var registration = new Registration
        {
            EventID = request.EventID,
            PublicUserID = publicUser.PublicUserID,
            RegistrationDate = DateTime.UtcNow,
            RegistrationStatus = "Confirmed"
        };

        _db.Registrations.Add(registration);
        await _db.SaveChangesAsync();

        return new RegistrationDto
        {
            RegistrationID = registration.RegistrationID,
            RegistrationDate = registration.RegistrationDate,
            RegistrationStatus = registration.RegistrationStatus,
            EventID = eventItem.EventID,
            EventName = eventItem.EventName,
            PublicUserID = publicUser.PublicUserID,
            FullName = publicUser.FullName,
            Email = publicUser.Email,
            PhoneNumber = publicUser.PhoneNumber
        };
    }

    public Task<bool> CheckRegistrationAsync(int publicUserId, int eventId)
    {
        return _db.Registrations.AnyAsync(x =>
            x.PublicUserID == publicUserId && x.EventID == eventId);
    }

    public async Task<List<RegistrationDto>> GetRegistrationsForEventAsync(int eventId, int organizerId)
    {
        var eventItem = await _db.Events.AsNoTracking().FirstOrDefaultAsync(x => x.EventID == eventId)
            ?? throw new KeyNotFoundException("Event not found.");

        if (eventItem.OrganizerID != organizerId)
        {
            throw new UnauthorizedAccessException("You can only view registrations for your own events.");
        }

        return await _db.Registrations
            .AsNoTracking()
            .Include(x => x.PublicUser)
            .Include(x => x.Event)
            .Where(x => x.EventID == eventId)
            .OrderBy(x => x.RegistrationDate)
            .Select(x => new RegistrationDto
            {
                RegistrationID = x.RegistrationID,
                RegistrationDate = x.RegistrationDate,
                RegistrationStatus = x.RegistrationStatus,
                EventID = x.EventID,
                EventName = x.Event != null ? x.Event.EventName : string.Empty,
                PublicUserID = x.PublicUserID,
                FullName = x.PublicUser != null ? x.PublicUser.FullName : string.Empty,
                Email = x.PublicUser != null ? x.PublicUser.Email : string.Empty,
                PhoneNumber = x.PublicUser != null ? x.PublicUser.PhoneNumber : string.Empty
            })
            .ToListAsync();
    }
}
