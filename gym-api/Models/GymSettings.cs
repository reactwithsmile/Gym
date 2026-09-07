namespace GymApi.Models;

public class GymSettings
{
    public int Id { get; set; }
    public string GymName { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string Currency { get; set; } = "INR";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
