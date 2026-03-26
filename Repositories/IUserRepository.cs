using TaskList.DTOs;
using TaskList.Models;

namespace TaskList.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<bool> CreateAsync(AuthUser user, string password);
    Task<bool> UpdateAsync(AuthUser user);
    Task<bool> DeleteAsync(string id);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task<bool> ResetPasswordAsync(string userId, string token, string newPassword);
    Task AddRefreshTokenAsync(User user, RefreshToken refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}
