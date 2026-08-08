using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using TaskList.Contexts;
using TaskList.DTOs;
using TaskList.Repositories;
using TaskList.Services;

namespace TaskList.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<TaskController> _logger;

    public TaskController(
        ITaskService taskService,
        IDistributedCache cache,
        ILogger<TaskController> logger
    )
    {
        _taskService = taskService;
        _cache = cache;
        _logger = logger;
    }

    // Create a new task with name and description (cached)
    [HttpPost("CreateTask")]
    [Authorize]
    public async Task<IActionResult> CreateTask(CreateTaskDTO Task)
    {
        var result = await _taskService.CreateAsync(Task);

        await _cache.RemoveAsync("tarefas:lista");
        _logger.LogInformation("🗑️ Cache da lista invalidado");

        return Ok(result);
    }

    // List all tasks (cached)
    [HttpGet("ListTasks")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ShowTaskDTO>>> ListTasks()
    {
        var cacheKey = $"tarefas:lista";

        try
        {
            // Tring use cache
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("📦 Lista de tarefas obtida do cache");
                var tarefas = JsonSerializer.Deserialize<List<ShowTaskDTO>>(cachedData);
                return Ok(tarefas);
            }

            // Searching on databse, has not found on cache
            _logger.LogInformation("🔄 Cache miss: buscando tarefas do banco");

            var tarefasDoBanco = await _taskService.GetAllAsync();

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5), // Expira em 5 min
            };

            // Saving new cache
            var json = JsonSerializer.Serialize(tarefasDoBanco);
            await _cache.SetStringAsync(cacheKey, json, options);

            _logger.LogInformation("✅ Lista de tarefas armazenada no cache");

            return Ok(tarefasDoBanco);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado");
            // Only return the database
            var tarefas = await _taskService.GetAllAsync();
            return Ok(tarefas);
        }
    }

    // Consult a task by id (cached)
    [HttpGet("ConsultTask/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> ConsultTask(int id)
    {
        var cacheKey = $"tarefa:{id}";
        ShowTaskDTO? tarefa = null;

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData is not null)
        {
            tarefa = JsonSerializer.Deserialize<ShowTaskDTO>(cachedData);
            _logger.LogInformation("Dados obtidos do cache para a chave {CacheKey}", cacheKey);
        }
        else
        {
            tarefa = await _taskService.GetByIdAsync(id);

            if (tarefa is not null)
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(tarefa), options);
                _logger.LogInformation(
                    "Dados armazenados no cache para a chave {CacheKey}",
                    cacheKey
                );
            }
        }

        if (tarefa != null)
        {
            return Ok(tarefa);
        }
        return NotFound();
    }

    // Update a task by id
    [HttpPut("UpdateTask/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskDTO updatedTask)
    {
        var update = await _taskService.UpdateAsync(id, updatedTask);

        await _cache.RemoveAsync($"tarefa:{id}");
        _logger.LogInformation("🗑️ Cache da lista invalidado");

        return Ok(update);
    }

    // Delete a task by id
    [HttpDelete("DeleteTask/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var delete = await _taskService.DeleteAsync(id);

        await _cache.RemoveAsync($"tarefa:{id}");
        _logger.LogInformation("🗑️ Cache da lista invalidado");

        if (delete)
        {
            return Ok($"Task {id} deleted sucefully!");
        }
        else
        {
            return NotFound("Task not found");
        }
    }

    // Update task status done
    [HttpPatch("ChangeStatus/{id}/{done}")]
    [Authorize]
    public async Task<IActionResult> ChangeStatusTask(int id, bool done)
    {
        var result = await _taskService.ChangeStatusAsync(id, done);

        await _cache.RemoveAsync($"tarefa:{id}");
        _logger.LogInformation("🗑️ Cache da lista invalidado");

        if (result == null)
        {
            return BadRequest("Task not found...");
        }
        return Ok(result);
    }
}
