using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskList.DTOs;
using TaskList.Services;

namespace TaskList.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "Permission:admin:access")]
public class AdminController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IPermissionService permissionService, ILogger<AdminController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetAllPermissions()
    {
        var permissions = await _permissionService.GetAllAvailablePermissionsAsync();
        return Ok(permissions);
    }

    [HttpGet("users/{userId}/permissions")]
    public async Task<IActionResult> GetUserPermissions(string userId)
    {
        var userPermissions = await _permissionService.GetUserPermissionsWithRolesAsync(userId);

        if (userPermissions == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(userPermissions);
    }

    [HttpPost("users/permissions")]
    [Authorize(Policy = "Permission:users:edit")]
    public async Task<IActionResult> AssignPermissions([FromBody] AssignPermissionsDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _permissionService.AssignPermissionsAsync(dto.UserId, dto.Permissions);

        if (!result)
        {
            return BadRequest(new { message = "Failed to assign permissions" });
        }

        return Ok(new { message = "Permissions assigned successfully" });
    }

    [HttpDelete("users/{userId}/permissions/{permission}")]
    [Authorize(Policy = "Permission:users:edit")]
    public async Task<IActionResult> RevokePermission(string userId, string permission)
    {
        var result = await _permissionService.RevokePermissionAsync(userId, permission);

        if (!result)
        {
            return BadRequest(new { message = "Failed to revoke permission" });
        }

        return Ok(new { message = "Permission revoked successfully" });
    }
}
