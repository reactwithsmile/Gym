namespace GymApi.DTOs;

public class AuthUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public IReadOnlyList<string> Permissions { get; set; } = [];
}
