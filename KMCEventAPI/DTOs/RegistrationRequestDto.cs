using System.ComponentModel.DataAnnotations;

namespace KMCEventAPI.DTOs;

public class RegistrationRequestDto
{
    [Range(1, int.MaxValue)]
    public int EventID { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;
}
