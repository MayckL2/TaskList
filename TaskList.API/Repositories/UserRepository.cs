using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskList.Contexts;
using TaskList.Models;

namespace TaskList.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    private readonly TaskContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        UserManager<User> userManager,
        TaskContext context,
        IMapper mapper,
        ILogger<UserRepository> logger
    )
    {
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        var token = await _context
            .Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.IsActive);

        return token?.User;
    }

    public async Task<bool> CreateAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            _logger.LogInformation("User created: {Email}", user.Email);
        }
        else
        {
            foreach (var error in result.Errors)
            {
                _logger.LogWarning(
                    "Error creating user {Email}: {Error}",
                    user.Email,
                    error.Description
                );
            }
        }

        return result.Succeeded;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await GetByIdAsync(id);
        if (user == null)
            return false;

        user.IsActive = false;
        user.DeletedAt = DateTime.UtcNow;

        return await UpdateAsync(user);
    }

    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<bool> ConfirmEmailAsync(User user, string token)
    {
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ResetPasswordAsync(User user, string token, string newPassword)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            await RevokeAllUserRefreshTokensAsync(user.Id);
            _logger.LogInformation("Password reset for user: {Email}", user.Email);
        }

        return result.Succeeded;
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _context.Set<RefreshToken>().AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var token = await _context
            .Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token != null && token.RevokedAt == null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllUserRefreshTokensAsync(string userId)
    {
        var tokens = await _context
            .Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IList<string>> GetUserRolesAsync(User user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> AddToRoleAsync(User user, string role)
    {
        var result = await _userManager.AddToRoleAsync(user, role);
        return result.Succeeded;
    }

    public async Task<bool> IsLockedOutAsync(User user)
    {
        return await _userManager.IsLockedOutAsync(user);
    }

    public async Task<int> IncrementAccessFailedCountAsync(User user)
    {
        var result = await _userManager.AccessFailedAsync(user);
        return await _userManager.GetAccessFailedCountAsync(user);
    }

    public async Task ResetAccessFailedCountAsync(User user)
    {
        await _userManager.ResetAccessFailedCountAsync(user);
    }

    // Add to existing UserRepository class
    public async Task<List<string>> GetUserPermissionsAsync(User user)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        return claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
    }

    public async Task<bool> AddPermissionAsync(User user, string permission)
    {
        var existingClaims = await _userManager.GetClaimsAsync(user);

        if (existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
            return true; // Already exists

        var result = await _userManager.AddClaimAsync(
            user,
            new System.Security.Claims.Claim("permission", permission)
        );

        return result.Succeeded;
    }

    public async Task<bool> AddPermissionsAsync(User user, IEnumerable<string> permissions)
    {
        var existingClaims = await _userManager.GetClaimsAsync(user);
        var newPermissions = permissions
            .Where(p => !existingClaims.Any(c => c.Type == "permission" && c.Value == p))
            .Select(p => new System.Security.Claims.Claim("permission", p))
            .ToList();

        if (!newPermissions.Any())
            return true;

        var result = await _userManager.AddClaimsAsync(user, newPermissions);
        return result.Succeeded;
    }

    public async Task<bool> RemovePermissionAsync(User user, string permission)
    {
        var claim = (await _userManager.GetClaimsAsync(user)).FirstOrDefault(c =>
            c.Type == "permission" && c.Value == permission
        );

        if (claim == null)
            return true;

        var result = await _userManager.RemoveClaimAsync(user, claim);
        return result.Succeeded;
    }

    public async Task<bool> HasPermissionAsync(User user, string permission)
    {
        var permissions = await GetUserPermissionsAsync(user);
        return permissions.Contains(permission);
    }

    public async Task<bool> ReplaceUserPermissionsAsync(User user, IEnumerable<string> permissions)
    {
        var existingClaims = (await _userManager.GetClaimsAsync(user))
            .Where(c => c.Type == "permission")
            .ToList();

        // Remove all existing permission claims
        foreach (var claim in existingClaims)
        {
            await _userManager.RemoveClaimAsync(user, claim);
        }

        // Add new permissions
        var newClaims = permissions
            .Select(p => new System.Security.Claims.Claim("permission", p))
            .ToList();

        if (newClaims.Any())
        {
            var result = await _userManager.AddClaimsAsync(user, newClaims);
            return result.Succeeded;
        }

        return true;
    }
}
