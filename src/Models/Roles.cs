namespace TaskList.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string NormalizedName { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navegação
    public ICollection<UserRole> UserRoles { get; set; }
}

public class UserRole
{
    public string UserId { get; set; }
    public int RoleId { get; set; }
    public DateTime AssignedAt { get; set; }

    // Navegação
    public User User { get; set; }
    public Role Role { get; set; }
}
