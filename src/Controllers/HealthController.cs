using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskList.Services;

namespace TaskList.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthService _healthService;

    public HealthController(HealthService healthService)
    {
        HealthService _healthService = healthService;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        return Ok(
            new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                message = "API is running!",
            }
        );
    }

    [HttpGet("detailed")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDetailed()
    {
        var health = new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            dependencies = new
            {
                database = _healthService.CheckDatabase(),
                memory = _healthService.CheckMemory(),
            },
        };

        return Ok(health);
    }
}
