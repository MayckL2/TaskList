namespace TaskList.DTOs;

public class AuthUser
{
    public string Id { get; set; }
    public string? FullName { get; set; }
    public string? PasswordHash { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Models/RefreshToken.cs
// public class RefreshToken
// {
//     public int Id { get; set; }
//     public string Token { get; set; }
//     public string JwtId { get; set; }
//     public int UserId { get; set; }
//     public DateTime CreatedAt { get; set; }
//     public DateTime ExpiresAt { get; set; }
//     public bool IsRevoked { get; set; }
//     public bool IsUsed { get; set; }
//     public AuthUser User { get; set; }
// }
