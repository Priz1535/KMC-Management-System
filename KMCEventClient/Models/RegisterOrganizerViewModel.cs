using System.ComponentModel.DataAnnotations;

namespace KMCEventClient.Models;

public class RegisterOrganizerViewModel
{
    [Required, Display(Name = "Organizer name"), MaxLength(100)]
    public string OrganizerName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone number"), MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, Display(Name = "Organization name"), MaxLength(150)]
    public string OrganizationName { get; set; } = string.Empty;

    [Required, MinLength(6), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
