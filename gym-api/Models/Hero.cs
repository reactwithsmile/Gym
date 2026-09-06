namespace GymApi.Models;

public class Hero
{
    public int Id { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PrimaryButtonText { get; set; } = string.Empty;
    public string PrimaryButtonLink { get; set; } = string.Empty;
    public string SecondaryButtonText { get; set; } = string.Empty;
    public string SecondaryButtonLink { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
