using Microsoft.AspNetCore.Mvc;
using TaskList.Contexts;
using TaskList.DTOs;
using TaskList.IServices;
using TaskList.Repositories;

namespace TaskList.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly TaskContext _context;
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService, TaskContext context)
    {
        _taskService = taskService;
        _context = context;
    }

    // Create a new task with name and description
    [HttpPost("CreateTask")]
    public async Task<IActionResult> CreateTask(CreateTaskDTO Task)
    {
        var result = await _taskService.CreateAsync(Task);
        return Ok(result);
    }

    // List all tasks
    [HttpGet("ListTasks")]
    public async Task<IActionResult> ListTasks()
    {
        return Ok(await _taskService.GetAllAsync());
    }

    // // Consult a task by id
    [HttpGet("ConsultTask/{id}")]
    public async Task<IActionResult> ConsultTask(int id)
    {
        var result = await _taskService.GetByIdAsync(id);
        if (result != null)
        {
            return Ok(result);
        }
        return NotFound();
    }

    // // Update a task by id
    [HttpPut("UpdateTask/{id}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskDTO updatedTask)
    {
        var update = await _taskService.UpdateAsync(id, updatedTask);
        return Ok(update);
    }

    // // Delete a task by id
    [HttpDelete("DeleteTask/{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var delete = await _taskService.DeleteAsync(id);
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
    [HttpPatch("CompleteTask/{id}/{done}")]
    public async Task<IActionResult> CompleteTask(int id, bool done)
    {
        var result = await _taskService.CompleteTaskAsync(id, done);
        if (result == null)
        {
            return BadRequest("Task not found...");
        }
        return Ok(result);
    }
}
