using Microsoft.AspNetCore.Mvc;
using TaskList.Contexts;
using TaskList.Entity;
using TaskList.Models;
using TaskList.DTOs;

namespace TaskList.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{

    private readonly TaskContext _context;

    public TaskController(TaskContext context)
    {
        _context = context;
    }

// Create a new task with name and description
    [HttpPost("CreateTask")]
    public IActionResult CreateTask(CreateTaskDTO Task)
    {
        TaskModel repository = new (
            Task.Title,
            Task.Description
        );
        _context.Tasks.Add(repository);
        _context.SaveChanges();

// Test with repository id returning 0
        Console.WriteLine($"ID antes de Add: {repository.Id}");
        
        return CreatedAtAction(nameof(CreateTask), new { id = repository.Id }, repository);
    }

// List all tasks
    [HttpGet("ListTasks")]
    public IActionResult ListTasks()
    {
        return Ok(_context.Tasks.ToList());
    }

// Consult a task by id
    [HttpGet("ConsultTask/{id}")]
    public IActionResult ConsultTask(int id)
    {
        var task = _context.Tasks.Find(id);
        if (task == null)
        {
            return NotFound();
        }
        return Ok(task);
    }

// Update a task by id
    [HttpPut("UpdateTask/{id}")]
    public IActionResult UpdateTask(int id, TaskDTO updatedTask)
    {
        var existingTask = _context.Tasks.Find(id);
        if (existingTask == null)
        {
            return NotFound();
        }

        existingTask.Title = updatedTask.Title;
        existingTask.Description = updatedTask.Description;
        existingTask.DateEdition = DateTime.Now;

        _context.SaveChanges();
        return Ok(existingTask);
    }

// Delete a task by id
    [HttpDelete("DeleteTask/{id}")]
    public IActionResult DeleteTask(int id)
    {
        var task = _context.Tasks.Find(id);
        if (task == null)
        {
            return NotFound();
        }
        _context.Tasks.Remove(task);
        _context.SaveChanges();
        return NoContent();
    }
}
