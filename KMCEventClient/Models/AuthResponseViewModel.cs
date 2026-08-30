namespace KMCEventClient.Models;

public class AuthResponseViewModel
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public OrganizerViewModel Organizer { get; set; } = new();
}

public class OrganizerViewModel
{
    public int OrganizerID { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
}
