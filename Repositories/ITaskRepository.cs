using TaskList.DTOs;

namespace TaskList.Repositories;

public interface ITaskRepository
{
    // 📖 Consults
    Task<ShowTaskDTO?> GetByIdAsync(int id);

    IQueryable<ShowTaskDTO> GetAllAsync();

    // Task<IEnumerable<TaskModel>> GetTaskUndoneAsync();
    // Task<TaskModel> GetByEmailAsync(string email);
    // Task<bool> ExistsByEmailAsync(string email);

    // ✍️ Write
    Task<ShowTaskDTO> CreateAsync(CreateTaskDTO task);
    Task<ShowTaskDTO> UpdateAsync(int id, UpdateTaskDTO task);
    Task<bool> DeleteAsync(int id); // Soft ou hard delete
    Task<ShowTaskDTO?> CompleteTaskAsync(ShowTaskDTO task, bool done);

    // // 📊 Pagination
    // Task<(IEnumerable<TaskModel> Itens, int Total)> GetPagedAsync(int page, int pageSize);
}
