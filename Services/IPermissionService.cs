using TaskList.DTOs;

namespace TaskList.Services;

public interface IPermissionService
{
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<bool> AssignPermissionsAsync(string userId, List<string> permissions);
    Task<bool> RevokePermissionAsync(string userId, string permission);
    Task<bool> HasPermissionAsync(string userId, string permission);
    Task<UserPermissionsResponseDto> GetUserPermissionsWithRolesAsync(string userId);
    Task<List<PermissionInfoDto>> GetAllAvailablePermissionsAsync();
}
