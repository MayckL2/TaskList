using System;
using Microsoft.EntityFrameworkCore;
using TaskList.DTOs;
using TaskList.IServices;
using TaskList.Repositories;

namespace TaskList.Services;

public class TaskService : ITaskService
{
    private readonly TaskRepository _repository;

    public TaskService(TaskRepository repository)
    {
        _repository = repository;
    }

    // Verify if all data required is fulled and do the creation
    public async Task<ShowTaskDTO> CreateAsync(CreateTaskDTO Task)
    {
        if (string.IsNullOrWhiteSpace(Task.Title))
        {
            throw new ArgumentException("Title is necessary", nameof(Task.Title));
        }

        if (string.IsNullOrWhiteSpace(Task.Description))
        {
            throw new ArgumentException("Description is necessary", nameof(Task.Description));
        }
        return await _repository.CreateAsync(Task);
    }

    // Get all task and return a list
    public async Task<List<ShowTaskDTO>> GetAllAsync()
    {
        return await _repository.GetAllAsync().ToListAsync();
    }

    // Try to find task by id, return the task or null
    public async Task<ShowTaskDTO?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    // Update task
    public async Task<ShowTaskDTO> UpdateAsync(int id, UpdateTaskDTO task)
    {
        return await _repository.UpdateAsync(id, task);
    }

    // Delete task if id exists and return bool
    public async Task<bool> DeleteAsync(int id)
    {
        var taskExist = await _repository.GetByIdAsync(id);
        if (taskExist == null)
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

    // Update task status done or return null if id was not find
    public async Task<ShowTaskDTO?> CompleteTaskAsync(int id, bool done)
    {
        var task = await _repository.GetByIdAsync(id);
        if (task == null)
            return null;

        return await _repository.CompleteTaskAsync(task, done);
    }
}
