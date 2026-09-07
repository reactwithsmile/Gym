namespace GymApi.DTOs;

public class GymSettingsDto
{
    public int Id { get; set; }
    public string GymName { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string Tagline { get; set; } = string.Empty;
    public string WebsiteUrl { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateGymSettingsRequest
{
    public string GymName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Tagline { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Currency { get; set; }
}

public class UpdateGymSettingsRequest : CreateGymSettingsRequest
{
}
