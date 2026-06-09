using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using TaskList.Contexts;
using TaskList.Models;

namespace TaskList.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly TaskContext _context;

    public RoleRepository(TaskContext context)
    {
        _context = context;
    }

    // 🔥 Buscar roles do usuário (substitui GetRolesAsync)
    public async Task<List<string>> GetUserRolesAsync(User user)
    {
        if (user == null)
            return new List<string>();

        var roles = await _context
            .UserRoles.Where(ur => ur.UserId == user.Id)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name)
            .ToListAsync();

        return roles;
    }

    // 🔥 Adicionar role a um usuário
    public async Task<bool> AddUserToRoleAsync(User user, string roleName)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

        if (role == null)
            return false;

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow,
        };

        await _context.UserRoles.AddAsync(userRole);
        return await _context.SaveChangesAsync() > 0;
    }

    // 🔥 Remover role de um usuário
    public async Task<bool> RemoveUserFromRoleAsync(User user, string roleName)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);

        if (role == null)
            return false;

        var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur =>
            ur.UserId == user.Id && ur.RoleId == role.Id
        );

        if (userRole == null)
            return false;

        _context.UserRoles.Remove(userRole);
        return await _context.SaveChangesAsync() > 0;
    }

    // 🔥 Verificar se usuário está em uma role
    public async Task<bool> IsUserInRoleAsync(User user, string roleName)
    {
        var roles = await GetUserRolesAsync(user);
        return roles.Contains(roleName);
    }

    // 🔥 Criar nova role
    public async Task<bool> CreateRoleAsync(string roleName)
    {
        if (await _context.Roles.AnyAsync(r => r.Name == roleName))
            return false;

        var role = new Role
        {
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow,
        };

        await _context.Roles.AddAsync(role);
        return await _context.SaveChangesAsync() > 0;
    }

    // 🔥 Listar todas as roles
    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }
}
