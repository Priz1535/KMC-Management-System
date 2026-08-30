namespace KMCEventAPI.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public OrganizerDto Organizer { get; set; } = new();
}
