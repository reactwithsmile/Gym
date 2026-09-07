namespace GymApi.DTOs;

public class TestimonialDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string RoleOrDescription { get; set; } = string.Empty;
    public string Review { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTestimonialRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string? RoleOrDescription { get; set; }
    public string Review { get; set; } = string.Empty;
    public int Rating { get; set; } = 5;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateTestimonialRequest
{
    public string? CustomerName { get; set; }
    public string? RoleOrDescription { get; set; }
    public string? Review { get; set; }
    public int? Rating { get; set; }
    public string? ImageUrl { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
}
