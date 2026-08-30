using System.ComponentModel.DataAnnotations;

namespace KMCEventAPI.Models;

public class Registration
{
    [Key]
    public int RegistrationID { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(30)]
    public string RegistrationStatus { get; set; } = "Confirmed";

    public int PublicUserID { get; set; }
    public int EventID { get; set; }

    public PublicUser? PublicUser { get; set; }
    public Event? Event { get; set; }
}
