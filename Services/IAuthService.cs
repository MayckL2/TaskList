using TaskList.DTOs;

namespace TaskList.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDTO registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto, string ipAddress);
    Task<AuthResponseDto> LogoutAsync(string refreshToken);
    Task<AuthResponseDto> ConfirmEmailAsync(string userId, string token);
    Task<AuthResponseDto> ForgotPasswordAsync(string email);
    Task<AuthResponseDto> ResetPasswordAsync(string email, string token, string newPassword);
    Task<UserResponseDto?> GetUserByIdAsync(string userId);
}
