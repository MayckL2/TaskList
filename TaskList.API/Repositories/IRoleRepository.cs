using TaskList.Models;

namespace TaskList.Repositories;

public interface IRoleRepository
{
    // ... métodos existentes

    // 🔥 Métodos de roles (substituem os do UserManager)
    Task<List<string>> GetUserRolesAsync(User user);
    Task<bool> AddUserToRoleAsync(User user, string roleName);
    Task<bool> RemoveUserFromRoleAsync(User user, string roleName);
    Task<bool> IsUserInRoleAsync(User user, string roleName);
    Task<bool> CreateRoleAsync(string roleName);
    Task<List<Role>> GetAllRolesAsync();
}
