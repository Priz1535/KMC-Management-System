using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KMCEventAPI.Data;
using KMCEventAPI.DTOs;
using KMCEventAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KMCEventAPI.Services;

public class AuthService
{
    private readonly KmcDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<EventOrganizer> _passwordHasher = new();

    public AuthService(KmcDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<(bool Success, string Message, OrganizerDto? Organizer)> RegisterOrganizerAsync(
        RegisterOrganizerDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await _db.EventOrganizers.AnyAsync(x => x.Email == normalizedEmail))
        {
            return (false, "An organizer account already exists with this email address.", null);
        }

        var organizer = new EventOrganizer
        {
            OrganizerName = request.OrganizerName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = request.PhoneNumber.Trim(),
            OrganizationName = request.OrganizationName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        organizer.PasswordHash = _passwordHasher.HashPassword(organizer, request.Password);

        _db.EventOrganizers.Add(organizer);
        await _db.SaveChangesAsync();

        return (true, "Organizer registered successfully.", ToDto(organizer));
    }

    public async Task<AuthResponseDto?> LoginOrganizerAsync(LoginDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var organizer = await _db.EventOrganizers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail);

        if (organizer is null)
        {
            return null;
        }

        var verification = _passwordHasher.VerifyHashedPassword(
            organizer,
            organizer.PasswordHash,
            request.Password);

        if (verification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var expiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var parsed)
            ? parsed
            : 120;
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        return new AuthResponseDto
        {
            Token = CreateToken(organizer, expiresAt),
            ExpiresAt = expiresAt,
            Organizer = ToDto(organizer)
        };
    }

    public Task<bool> AuthenticateOrganizerAsync(int organizerId)
    {
        return _db.EventOrganizers.AsNoTracking().AnyAsync(x => x.OrganizerID == organizerId);
    }

    private string CreateToken(EventOrganizer organizer, DateTime expiresAt)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is missing from configuration.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, organizer.OrganizerID.ToString()),
            new(ClaimTypes.Name, organizer.OrganizerName),
            new(ClaimTypes.Email, organizer.Email),
            new("organization", organizer.OrganizationName)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static OrganizerDto ToDto(EventOrganizer organizer) => new()
    {
        OrganizerID = organizer.OrganizerID,
        OrganizerName = organizer.OrganizerName,
        Email = organizer.Email,
        PhoneNumber = organizer.PhoneNumber,
        OrganizationName = organizer.OrganizationName
    };
}
