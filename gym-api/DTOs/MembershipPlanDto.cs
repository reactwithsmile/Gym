namespace GymApi.DTOs;

public class MembershipPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMonths { get; set; }
    public string Features { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPopular { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateMembershipPlanRequest
{
    public string Name { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationMonths { get; set; }
    public string? Features { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPopular { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateMembershipPlanRequest
{
    public string? Name { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? DurationMonths { get; set; }
    public string? Features { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsPopular { get; set; }
    public bool? IsActive { get; set; }
}
