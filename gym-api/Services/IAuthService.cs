using GymApi.DTOs;

namespace GymApi.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthUserDto?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);
}
