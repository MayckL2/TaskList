using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TaskList.Contexts;
using TaskList.DTOs;
using TaskList.Models;

namespace TaskList.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TaskContext _context;
    private readonly IMapper _mapper;

    public TaskRepository(TaskContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // Search by Id and return the task or null
    public async Task<ShowTaskDTO?> GetByIdAsync(int id)
    {
        var query = await _context.Tasks.FirstOrDefaultAsync(c => c.Id == id);
        return _mapper.Map<ShowTaskDTO>(query);
    }

    // Return all task
    public IQueryable<ShowTaskDTO> GetAllAsync()
    {
        return _context.Tasks.ProjectTo<ShowTaskDTO>(_mapper.ConfigurationProvider);
    }

    // Register task on database and return criation
    public async Task<ShowTaskDTO> CreateAsync(CreateTaskDTO Task)
    {
        TaskModel task = _mapper.Map<TaskModel>(Task);

        task.DateCreation = DateTime.Now;
        task.DateEdition = DateTime.Now;

        _context.Tasks.Add(task);

        int affectedRows = await _context.SaveChangesAsync();
        if (affectedRows > 0)
        {
            Console.WriteLine($"Id da task: {task.Id}");
        }
        return _mapper.Map<ShowTaskDTO>(task);
    }

    // Update task on database and return the modification
    public async Task<ShowTaskDTO> UpdateAsync(int id, UpdateTaskDTO task)
    {
        var existingTask = _context.Tasks.Find(id);
        if (existingTask == null)
        {
            throw new KeyNotFoundException($"Task ID {id} not found.");
        }

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.DateEdition = DateTime.Now;

        _context.SaveChanges();
        var updatedTask = await this.GetByIdAsync(id);
        if (updatedTask == null)
        {
            throw new KeyNotFoundException($"Task ID {id} not found after updated.");
        }
        return updatedTask;
    }

    // Delete task from database
    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return false;
        }
        _context.Tasks.Remove(task);
        _context.SaveChanges();
        return true;
    }

    // Uptade status done of the task
    public async Task<ShowTaskDTO?> CompleteTaskAsync(ShowTaskDTO task, bool done)
    {
        task.Done = done;
        _context.SaveChanges();

        return task;
    }
}
