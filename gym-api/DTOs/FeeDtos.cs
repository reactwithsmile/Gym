namespace GymApi.DTOs;

public record MemberRequest(string Name, string Email, string? Phone, bool IsActive = true);
public record MembershipRequest(int MemberId, int? MembershipPlanId, DateTime StartDate, DateTime EndDate);
public record PaymentRequest(int MemberId, int? MembershipId, decimal Amount, DateTime? PaidAt, string Method, string? Reference, string? Status = "Paid", string? Notes = null);
public record NotificationRequest(string Title, string Message, string? Type);
