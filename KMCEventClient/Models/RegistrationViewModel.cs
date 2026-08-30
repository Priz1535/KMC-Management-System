namespace KMCEventClient.Models;

public class RegistrationViewModel
{
    public int RegistrationID { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string RegistrationStatus { get; set; } = string.Empty;
    public int EventID { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int PublicUserID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
