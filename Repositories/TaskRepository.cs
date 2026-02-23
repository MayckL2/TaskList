using AutoMapper;
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

    // Search by Id
    public async Task<ShowTaskDTO?> GetByIdAsync(int id)
    {
        var query = await _context.Tasks.FirstOrDefaultAsync(c => c.Id == id);
        return _mapper.Map<ShowTaskDTO>(query);
    }

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

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        _context.Tasks.Remove(task);
        _context.SaveChanges();
        return true;
    }
}
