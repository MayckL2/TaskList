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

    public async Task<List<ShowTaskDTO>> GetAllAsync()
    {
        return await _repository.GetAllAsync().ToListAsync();
    }

    public async Task<ShowTaskDTO?> GetByIdAsync(int id)
    {
        var query = await _repository.GetByIdAsync(id);
        return query;
    }

    public async Task<ShowTaskDTO> UpdateAsync(int id, UpdateTaskDTO task)
    {
        var query = await _repository.UpdateAsync(id, task);
        return query;
    }

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
}
