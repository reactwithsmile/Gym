namespace GymApi.DTOs;

public class AboutDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public int MembersCount { get; set; }
    public int TrainersCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateAboutRequest
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int ExperienceYears { get; set; }
    public int MembersCount { get; set; }
    public int TrainersCount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateAboutRequest
{
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ExperienceYears { get; set; }
    public int? MembersCount { get; set; }
    public int? TrainersCount { get; set; }
    public bool? IsActive { get; set; }
}
