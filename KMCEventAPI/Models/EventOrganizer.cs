using System.ComponentModel.DataAnnotations;

namespace KMCEventAPI.Models;

public class EventOrganizer
{
    [Key]
    public int OrganizerID { get; set; }

    [Required, MaxLength(100)]
    public string OrganizerName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string OrganizationName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
