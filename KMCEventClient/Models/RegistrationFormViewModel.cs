using System.ComponentModel.DataAnnotations;

namespace KMCEventClient.Models;

public class RegistrationFormViewModel
{
    [Range(1, int.MaxValue)]
    public int EventID { get; set; }

    public string EventName { get; set; } = string.Empty;

    [Required, Display(Name = "Full name"), MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone number"), MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;
}
