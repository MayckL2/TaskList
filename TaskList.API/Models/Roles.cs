namespace TaskList.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string NormalizedName { get; set; }  = String.Empty;
    public string Description { get; set; } = String.Empty;
    public DateTime CreatedAt { get; set; }

    // Navegação
    // public ICollection<UserRole> UserRoles { get; set; }
}

public class UserRole
{
    public required string UserId { get; set; }
    public int RoleId { get; set; }
    public DateTime AssignedAt { get; set; }

    // Navegação
    public required User User { get; set; }
    public required Role Role { get; set; }
}
