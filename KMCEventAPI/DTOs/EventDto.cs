namespace KMCEventAPI.DTOs;

public class EventDto
{
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public TimeSpan EventTime { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int RegisteredCount { get; set; }
    public int AvailablePlaces => Math.Max(0, Capacity - RegisteredCount);
    public DateTime CreatedAt { get; set; }
    public int OrganizerID { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public int VenueID { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public string VenueAddress { get; set; } = string.Empty;
    public string VenueCity { get; set; } = string.Empty;
}
