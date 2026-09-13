namespace GymApi.Models;

public class Membership
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int? MembershipPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public Member Member { get; set; } = null!;
    public MembershipPlan? MembershipPlan { get; set; }
    public ICollection<Payment> Payments { get; set; } = [];
}
