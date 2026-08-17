using System.ComponentModel.DataAnnotations;

namespace TaskList.DTOs;

public class AssignRoleDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}

public class UserRolesResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

public class ReplaceRolesDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public List<string> Roles { get; set; } = new();
}
