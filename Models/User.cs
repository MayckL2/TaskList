using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TaskList.Models;

public class User : IdentityUser
{
    [StringLength(100)]
    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Refresh tokens
    public List<RefreshToken> RefreshTokens { get; set; } = new();

    // Claims
    public ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();

    // Soft delete
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
}

public class UserClaim
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}

// Model para Refresh Token
public class RefreshToken
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }

    // Navigation property
    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsExpired && RevokedAt == null;
}
