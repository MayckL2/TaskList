using Microsoft.AspNetCore.Mvc;
using TaskList.Models;

namespace TaskList.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{

// Create a list to store tasks
    public List<TaskModel> Tasks = new List<TaskModel>
    {
        new TaskModel("lavar a louça", "usar detergente e esponja"),
        new TaskModel("estudar C#", "focar nas aulas do curso"),
        new TaskModel("fazer compras", "ir ao supermercado comprar mantimentos")
    };

// Create a new task with name and description
    [HttpGet("CreateTask/{name}/{description}")]
    public IActionResult CreateTask(string name, string description)
    {
        TaskModel task = new(name, description);
        this.Tasks.Add(task);
        return Ok(this.Tasks);
    }

// List all tasks
    [HttpGet("ListTasks")]
    public IActionResult ListTasks()
    {
        return Ok(this.Tasks);
    }
}
