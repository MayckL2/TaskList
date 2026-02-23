using TaskList.DTOs;
using TaskList.Models;

namespace TaskList.Repositories;

public interface ITaskRepository
{
    // 📖 Consults
    Task<ShowTaskDTO?> GetByIdAsync(int id);

    // Task<IEnumerable<TaskModel>> GetAllAsync();
    // Task<IEnumerable<TaskModel>> GetTaskUndoneAsync();
    // Task<TaskModel> GetByEmailAsync(string email);
    // Task<bool> ExistsByEmailAsync(string email);

    // ✍️ Write
    // Task AddAsync(TaskModel TaskModel);
    Task<ShowTaskDTO> UpdateAsync(int id, UpdateTaskDTO task);
    Task<bool> DeleteAsync(int id); // Soft ou hard delete
    // Task SaveChangesAsync();

    // // 📊 Pagination
    // Task<(IEnumerable<TaskModel> Itens, int Total)> GetPagedAsync(int page, int pageSize);
}
