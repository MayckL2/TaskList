using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskList.Services;

namespace TaskList.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IHealthService _healthService;

    public HealthController(IHealthService healthService)
    {
        _healthService = healthService;
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
