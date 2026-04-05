using TaskList.DTOs;
using TaskList.Models;
using TaskList.Repositories;

namespace TaskList.Services;

public class PermissionService : IPermissionService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(IUserRepository userRepository, ILogger<PermissionService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return new List<string>();

        return await _userRepository.GetUserPermissionsAsync(user);
    }

    public async Task<bool> AssignPermissionsAsync(string userId, List<string> permissions)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        // Validate permissions exist
        var invalidPermissions = permissions.Except(Permissions.All).ToList();
        if (invalidPermissions.Any())
        {
            _logger.LogWarning(
                "Invalid permissions attempted: {Invalid}",
                string.Join(", ", invalidPermissions)
            );
            return false;
        }

        var result = await _userRepository.ReplaceUserPermissionsAsync(user, permissions);

        if (result)
        {
            _logger.LogInformation(
                "Permissions assigned to user {UserId}: {Permissions}",
                userId,
                string.Join(", ", permissions)
            );
        }

        return result;
    }

    public async Task<bool> RevokePermissionAsync(string userId, string permission)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _userRepository.RemovePermissionAsync(user, permission);

        if (result)
        {
            _logger.LogInformation(
                "Permission {Permission} revoked from user {UserId}",
                permission,
                userId
            );
        }

        return result;
    }

    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        return await _userRepository.HasPermissionAsync(user, permission);
    }

    public async Task<UserPermissionsResponseDto> GetUserPermissionsWithRolesAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return null!;

        var permissions = await _userRepository.GetUserPermissionsAsync(user);
        var roles = await _userRepository.GetUserRolesAsync(user);

        return new UserPermissionsResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Permissions = permissions,
            Roles = roles.ToList(),
        };
    }

    public Task<List<PermissionInfoDto>> GetAllAvailablePermissionsAsync()
    {
        var permissions = new List<PermissionInfoDto>
        {
            new()
            {
                Name = Permissions.UsersView,
                Module = "Users",
                Description = "View user list and details",
            },
            new()
            {
                Name = Permissions.UsersCreate,
                Module = "Users",
                Description = "Create new users",
            },
            new()
            {
                Name = Permissions.UsersEdit,
                Module = "Users",
                Description = "Edit user information",
            },
            new()
            {
                Name = Permissions.UsersDelete,
                Module = "Users",
                Description = "Delete users",
            },
            new()
            {
                Name = Permissions.TasksView,
                Module = "Tasks",
                Description = "View tasks",
            },
            new()
            {
                Name = Permissions.TasksCreate,
                Module = "Tasks",
                Description = "Create new tasks",
            },
            new()
            {
                Name = Permissions.TasksEdit,
                Module = "Tasks",
                Description = "Edit tasks",
            },
            new()
            {
                Name = Permissions.TasksDelete,
                Module = "Tasks",
                Description = "Delete tasks",
            },
            new()
            {
                Name = Permissions.TasksAssign,
                Module = "Tasks",
                Description = "Assign tasks to users",
            },
            new()
            {
                Name = Permissions.ReportsView,
                Module = "Reports",
                Description = "View reports",
            },
            new()
            {
                Name = Permissions.ReportsExport,
                Module = "Reports",
                Description = "Export reports",
            },
            new()
            {
                Name = Permissions.ReportsSchedule,
                Module = "Reports",
                Description = "Schedule reports",
            },
            new()
            {
                Name = Permissions.AdminAccess,
                Module = "Admin",
                Description = "Access admin panel",
            },
            new()
            {
                Name = Permissions.AuditView,
                Module = "Admin",
                Description = "View audit logs",
            },
            new()
            {
                Name = Permissions.SystemConfig,
                Module = "Admin",
                Description = "Configure system settings",
            },
        };

        return Task.FromResult(permissions);
    }
}
