using System.ComponentModel.DataAnnotations;

namespace TaskList.Models;

public class User
{
    public string Id { get; set; }

    [StringLength(100)]
    public string? FullName { get; set; }
    public string  Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? Role { get; set; }

    // Refresh tokens
    public List<RefreshToken> RefreshTokens { get; set; } = new();

    // Roles
    public ICollection<UserRole> UserRoles { get; set; }

    // Soft delete
    public bool IsActive { get; set; } = true;
    public DateTime? DeletedAt { get; set; }
}

// Model para Refresh Token
public class RefreshToken
{
    public string Id { get; set; }
    public string Token { get; set; }
    public DateTime Expires { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string JwtId { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }

    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsActive => !IsExpired && RevokedAt == null;
    public bool IsUsed { get; set; }
}
