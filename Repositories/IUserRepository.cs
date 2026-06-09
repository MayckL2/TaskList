using TaskList.Models;

namespace TaskList.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<bool> CreateAsync(User user, string password);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(string id);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
    Task<bool> ConfirmEmailAsync(User user, string token);
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task<bool> ResetPasswordAsync(User user, string token, string newPassword);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task RevokeAllUserRefreshTokensAsync(string userId);
    Task<IList<string>> GetUserRolesAsync(User user);
    Task<bool> AddToRoleAsync(User user, string role);
    Task<bool> IsLockedOutAsync(User user);
    Task<int> IncrementAccessFailedCountAsync(User user);
    Task ResetAccessFailedCountAsync(User user);

    // Claims
    Task<List<string>> GetUserPermissionsAsync(User user);
    Task<bool> AddPermissionAsync(User user, string permission);
    Task<bool> AddPermissionsAsync(User user, IEnumerable<string> permissions);
    Task<bool> RemovePermissionAsync(User user, string permission);
    Task<bool> HasPermissionAsync(User user, string permission);
    Task<bool> ReplaceUserPermissionsAsync(User user, IEnumerable<string> permissions);
}
