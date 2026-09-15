using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskList.DTOs;
using TaskList.Models;

namespace TaskList.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<RolesController> _logger;

    public RolesController(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<RolesController> logger
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    // Get All Roles on database
    [HttpGet("all")]
    public IActionResult GetAllRoles()
    {
        var roles = _roleManager.Roles.Select(r => r.Name).ToList();
        return Ok(roles);
    }

    // Get user with his roles
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserRoles(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { message = "Usuário não encontrado" });

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(
            new UserRolesResponseDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles.ToList(),
            }
        );
    }

    // Assign role to a user by his ID
    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return NotFound(new { message = "Usuário não encontrado" });

        if (!await _roleManager.RoleExistsAsync(dto.Role))
            return BadRequest(new { message = $"Role '{dto.Role}' não existe" });

        if (await _userManager.IsInRoleAsync(user, dto.Role))
            return BadRequest(new { message = $"Usuário já possui a role '{dto.Role}'" });

        var result = await _userManager.AddToRoleAsync(user, dto.Role);

        if (!result.Succeeded)
            return BadRequest(new { message = "Erro ao atribuir role", errors = result.Errors });

        _logger.LogInformation("Role '{Role}' atribuída ao usuário {User}", dto.Role, user.Email);

        return Ok(
            new
            {
                message = $"Role '{dto.Role}' atribuída com sucesso",
                user = new
                {
                    user.Id,
                    user.Email,
                    user.UserName,
                },
            }
        );
    }

    // Remove role from user by his ID
    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return NotFound(new { message = "Usuário não encontrado" });

        if (!await _userManager.IsInRoleAsync(user, dto.Role))
            return BadRequest(new { message = $"Usuário não possui a role '{dto.Role}'" });

        var result = await _userManager.RemoveFromRoleAsync(user, dto.Role);

        if (!result.Succeeded)
            return BadRequest(new { message = "Erro ao remover role", errors = result.Errors });

        _logger.LogInformation("Role '{Role}' removida do usuário {User}", dto.Role, user.Email);

        return Ok(new { message = $"Role '{dto.Role}' removida com sucesso" });
    }

    // Change all roles from a user at once by his ID
    [HttpPut("replace")]
    public async Task<IActionResult> ReplaceRoles([FromBody] ReplaceRolesDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
            return NotFound(new { message = "Usuário não encontrado" });

        // Validate Roles
        foreach (var role in dto.Roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                return BadRequest(new { message = $"Role '{role}' não existe" });
        }

        // Get actual roles
        var currentRoles = await _userManager.GetRolesAsync(user);

        // Remove all
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Add news
        if (dto.Roles.Any())
        {
            await _userManager.AddToRolesAsync(user, dto.Roles);
        }

        _logger.LogInformation(
            "Roles do usuário {User} substituídas: {Roles}",
            user.Email,
            string.Join(", ", dto.Roles)
        );

        return Ok(new { message = "Roles substituídas com sucesso", roles = dto.Roles });
    }
}
