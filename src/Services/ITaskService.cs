using Gridify;
using TaskList.DTOs;
using TaskList.Models;

namespace TaskList.Services
{
    public interface ITaskService
    {
        Task<ShowTaskDTO> CreateAsync(CreateTaskDTO task);
        Task<List<ShowTaskDTO>> GetAllAsync();
        Task<ShowTaskDTO?> GetByIdAsync(int id);
        Task<ShowTaskDTO> UpdateAsync(UpdateTaskDTO task);
        Task<bool> DeleteAsync(int id);
        Task<ShowTaskDTO?> ChangeStatusAsync(int id, bool done);
        Task<Paging<ShowTaskDTO>> GetFilteredTasksAsync(GridifyQuery gridifyQuery);
    }
}
