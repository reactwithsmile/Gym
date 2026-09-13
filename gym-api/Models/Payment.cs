namespace GymApi.Models;

public class Payment
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int? MembershipId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string Method { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Status { get; set; } = "Paid";
    public string Notes { get; set; } = string.Empty;
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public User? CreatedByUser { get; set; }
    public Member Member { get; set; } = null!;
    public Membership? Membership { get; set; }
}
