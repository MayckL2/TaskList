using System;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskList.Contexts;
using TaskList.DTOs;
using TaskList.IServices;
using TaskList.Models;
using TaskList.Repositories;

namespace TaskList.Services;

public class TaskService : ITaskService
{
    private readonly TaskContext _context;
    private readonly IMapper _mapper;
    private readonly TaskRepository _repository;

    public TaskService(TaskContext context, IMapper mapper, TaskRepository repository)
    {
        _context = context;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<TaskModel> CreateAsync(CreateTaskDTO Task)
    {
        if (string.IsNullOrWhiteSpace(Task.Title))
        {
            throw new ArgumentException("Title is necessary", nameof(Task.Title));
        }

        if (string.IsNullOrWhiteSpace(Task.Description))
        {
            throw new ArgumentException("Description is necessary", nameof(Task.Description));
        }

        TaskModel task = new(Task.Title, Task.Description);

        _context.Tasks.Add(task);

        int affectedRows = await _context.SaveChangesAsync();
        if (affectedRows > 0)
        {
            Console.WriteLine($"Id da task: {task.Id}");
        }
        return task;
    }

    public async Task<IEnumerable<ShowTaskDTO>> GetAllAsync()
    {
        return await _context
            .Tasks.Select(t => new ShowTaskDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
            })
            .ToListAsync();
    }

    public async Task<ShowTaskDTO?> GetByIdAsync(int id)
    {
        var query = await _repository.GetByIdAsync(id);
        return query;
        // return await _context
        //     .Tasks.Where(t => t.Id == id)
        //     .Select(t => new ShowTaskDTO
        //     {
        //         Id = t.Id,
        //         Title = t.Title,
        //         Description = t.Description,
        //         Done = t.Done,
        //     })
        //     .FirstOrDefaultAsync();
    }

    public async Task<ShowTaskDTO> UpdateAsync(int id, UpdateTaskDTO task)
    {
        var query = await _repository.UpdateAsync(id, task);
        return query;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var taskExist = await _repository.GetByIdAsync(id);
        if (taskExist != null)
        {
            return false;
        }
        var deleted = await _repository.DeleteAsync(id);
        if (deleted)
        {
            return true;
        }
        return false;
    }
}
