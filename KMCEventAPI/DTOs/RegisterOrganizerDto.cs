using System.ComponentModel.DataAnnotations;

namespace KMCEventAPI.DTOs;

public class RegisterOrganizerDto
{
    [Required, MaxLength(100)]
    public string OrganizerName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string OrganizationName { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
