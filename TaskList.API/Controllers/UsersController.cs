using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskList.DTOs;
using TaskList.Models;

namespace TaskList.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserManager<User> userManager, ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    // Get all users (only Admin)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager
            .Users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                FullName = u.FullName ?? string.Empty,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                IsActive = u.IsActive,
            })
            .ToListAsync();

        // Search by roles for each user
        var userRoles = new Dictionary<string, List<string>>();
        foreach (var user in await _userManager.Users.ToListAsync())
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRoles[user.Id] = roles.ToList();
        }

        // Add roles to the answer
        foreach (var userDto in users)
        {
            if (userRoles.TryGetValue(userDto.Id, out var roles))
            {
                userDto.Roles = roles;
            }
        }

        _logger.LogInformation("📊 Lista de usuários consultada por {User}", User.Identity?.Name);

        return Ok(new { total = users.Count, users = users });
    }

    // Get all users with pagination (recommende for a lot of users)
    [HttpGet("paged")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsersPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null
    )
    {
        var query = _userManager.Users.AsQueryable();

        // filter by name or email
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u =>
                u.UserName!.Contains(search)
                || u.Email!.Contains(search)
                || u.FullName!.Contains(search)
            );
        }

        var total = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                FullName = u.FullName ?? string.Empty,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                IsActive = u.IsActive,
            })
            .ToListAsync();

        // ########### KEEP COMMENTED, CAUSING ERROR ###########

        // Search by roles (prevent N+1 with only one consult)
        // var userIds = users.Select(u => u.Id).ToList();
        // var allUserRoles = await _userManager
        //     .Users.Where(u => userIds.Contains(u.Id))
        //     .SelectMany(u =>
        //         _userManager.GetRolesAsync(u).Result.Select(r => new { UserId = u.Id, Role = r })
        //     )
        //     .ToListAsync();

        // Map roles for the DTOs
        // foreach (var user in users)
        // {
        //     user.Roles = allUserRoles
        //         .Where(ur => ur.UserId == user.Id)
        //         .Select(ur => ur.Role)
        //         .ToList();
        // }

        return Ok(
            new
            {
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)total / pageSize),
                users,
            }
        );
    }

    // Search user by ID (admin or the user it self)
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetUserById(string id)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isAdmin = User.IsInRole("Admin");

        // Admin can see everybody, user can only see his self
        if (!isAdmin && currentUserId != id)
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuário não encontrado" });

        var roles = await _userManager.GetRolesAsync(user);

        var userDto = new UserResponseDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            UpdatedAt = user.UpdatedAt,
            Roles = roles.ToList(),
        };

        return Ok(userDto);
    }

    // Get only active users (filter)
    [HttpGet("active")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetActiveUsers()
    {
        var users = await _userManager
            .Users.Where(u => u.IsActive)
            .Select(u => new UserResponseDto
            {
                Id = u.Id,
                UserName = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                FullName = u.FullName ?? string.Empty,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                IsActive = u.IsActive,
            })
            .ToListAsync();

        return Ok(users);
    }
}
