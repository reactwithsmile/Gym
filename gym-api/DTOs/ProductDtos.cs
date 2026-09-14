namespace GymApi.DTOs;

public record ProductRequest(string Name, string Category, string Description, string? ImageUrl, decimal Price, bool IsActive, int DisplayOrder);
