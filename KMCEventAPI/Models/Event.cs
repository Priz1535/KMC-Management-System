using System.ComponentModel.DataAnnotations;

namespace KMCEventAPI.Models;

public class Event
{
    [Key]
    public int EventID { get; set; }

    [Required, MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    [Required, MaxLength(1500)]
    public string Description { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public TimeSpan EventTime { get; set; }

    [Required, MaxLength(80)]
    public string EventType { get; set; } = string.Empty;

    [Range(1, 100000)]
    public int Capacity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int OrganizerID { get; set; }
    public int VenueID { get; set; }

    public EventOrganizer? Organizer { get; set; }
    public Venue? Venue { get; set; }
    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
