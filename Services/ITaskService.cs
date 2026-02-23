using TaskList.DTOs;
using TaskList.Models;

namespace TaskList.IServices
{
    public interface ITaskService
    {
        Task<TaskModel> CreateAsync(CreateTaskDTO task);
        Task<IEnumerable<ShowTaskDTO>> GetAllAsync();
        Task<ShowTaskDTO?> GetByIdAsync(int id);
        Task<ShowTaskDTO> UpdateAsync(int id, UpdateTaskDTO task);
        Task<bool> DeleteAsync(int id);
    }
}
