namespace GymApi.DTOs;

public record CreateEnquiryRequest(string Name, string Email, string? Phone, string Message);
public record UpdateEnquiryRequest(string Status, string? Reason);
