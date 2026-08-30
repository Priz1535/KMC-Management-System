using System.ComponentModel.DataAnnotations;

namespace KMCEventAPI.DTOs;

public class EventUpsertDto
{
    [Required, MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    [Required, MaxLength(1500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    public TimeSpan EventTime { get; set; }

    [Required, MaxLength(80)]
    public string EventType { get; set; } = string.Empty;

    [Range(1, 100000)]
    public int Capacity { get; set; }

    [Range(1, int.MaxValue)]
    public int VenueID { get; set; }
}
