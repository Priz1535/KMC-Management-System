using System.ComponentModel.DataAnnotations;

namespace KMCEventClient.Models;

public class EventFormViewModel
{
    public int EventID { get; set; }

    [Required, Display(Name = "Event name"), MaxLength(150)]
    public string EventName { get; set; } = string.Empty;

    [Required, MaxLength(1500)]
    public string Description { get; set; } = string.Empty;

    [Required, DataType(DataType.Date), Display(Name = "Event date")]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(7);

    [Required, Display(Name = "Event time")]
    public TimeSpan EventTime { get; set; } = new(9, 0, 0);

    [Required, Display(Name = "Event type"), MaxLength(80)]
    public string EventType { get; set; } = string.Empty;

    [Range(1, 100000)]
    public int Capacity { get; set; }

    [Range(1, int.MaxValue), Display(Name = "Venue")]
    public int VenueID { get; set; }

    public List<VenueViewModel> Venues { get; set; } = new();
}
