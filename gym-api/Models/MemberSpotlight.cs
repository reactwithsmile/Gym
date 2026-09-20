namespace GymApi.Models;

public class MemberSpotlight
{
    public int Id { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string Quote { get; set; } = string.Empty;
    public string Story { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string DurationText { get; set; } = string.Empty;
    public string AchievementText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
