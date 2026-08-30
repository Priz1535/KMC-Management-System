using System.ComponentModel.DataAnnotations;

namespace KMCEventAPI.Models;

public class Venue
{
    [Key]
    public int VenueID { get; set; }

    [Required, MaxLength(150)]
    public string VenueName { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Address { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Range(1, 100000)]
    public int Capacity { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
