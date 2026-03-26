using TaskList.DTOs;
using static TaskList.DTOs.RegisterDTO;

namespace TaskList.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDTO registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    Task<bool> LogoutAsync(string userId, string refreshToken);
    Task<UserResponseDto> GetUserProfileAsync(string userId);
}
