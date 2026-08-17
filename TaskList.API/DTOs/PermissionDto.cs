using System.ComponentModel.DataAnnotations;

namespace TaskList.DTOs;

public class AssignPermissionsDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public List<string> Permissions { get; set; } = new();
}

public class UserPermissionsResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public List<string> Roles { get; set; } = new();
}

public class PermissionInfoDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}
