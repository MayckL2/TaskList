using TaskList.DTOs;
using TaskList.Models;

namespace TaskList.IServices
{
    public interface ITaskService
    {
        Task<ShowTaskDTO> CreateAsync(CreateTaskDTO task);
        Task<List<ShowTaskDTO>> GetAllAsync();
        Task<ShowTaskDTO?> GetByIdAsync(int id);
        Task<ShowTaskDTO> UpdateAsync(int id, UpdateTaskDTO task);
        Task<bool> DeleteAsync(int id);
    }
}
