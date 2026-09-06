namespace GymApi.DTOs;

public class HeroDto
{
    public int Id { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PrimaryButtonText { get; set; } = string.Empty;
    public string PrimaryButtonLink { get; set; } = string.Empty;
    public string SecondaryButtonText { get; set; } = string.Empty;
    public string SecondaryButtonLink { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateHeroRequest
{
    public string Heading { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PrimaryButtonText { get; set; } = string.Empty;
    public string PrimaryButtonLink { get; set; } = string.Empty;
    public string SecondaryButtonText { get; set; } = string.Empty;
    public string SecondaryButtonLink { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class UpdateHeroRequest
{
    public string? Heading { get; set; }
    public string? Description { get; set; }
    public string? PrimaryButtonText { get; set; }
    public string? PrimaryButtonLink { get; set; }
    public string? SecondaryButtonText { get; set; }
    public string? SecondaryButtonLink { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; }
}
