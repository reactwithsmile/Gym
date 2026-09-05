namespace GymApi.DTOs;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public AuthUserDto User { get; set; } = null!;
}
