using HotChocolate;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using TaskList.DTOs;
using TaskList.Models;
using TaskList.Services;

namespace TaskList.GraphQL;

public class TaskQuery
{
    // 🔥 Resolver to return all tasks
    public async Task<IEnumerable<ShowTaskDTO?>> GetTasksAsync(
        [Service] ITaskService taskService,
        CancellationToken cancellationToken
    )
    {
        var tarefas = await taskService.GetAllAsync();
        return tarefas;
    }

    // 🔥 Resolver with parameter - get by id
    public async Task<ShowTaskDTO?> GetTaskByIdAsync(
        int id,
        [Service] ITaskService taskService,
        CancellationToken cancellationToken
    )
    {
        var tarefa = await taskService.GetByIdAsync(id);

        if (tarefa != null)
        {
            return tarefa;
        }
        return null;
    }

    // 🔥 Resolver with filters (pagination)
    // public async Task<IEnumerable<ShowTaskDTO>> GetTasksByStatusAsync(
    //     bool Done,
    //     [Service] ITaskService context,
    //     CancellationToken cancellationToken
    // )
    // {
    //     return await context
    //         .Tasks.Where(t => t.Done == Done)
    //         .Select(t => new TaskDto
    //         {
    //             Id = t.Id,
    //             Title = t.Title,
    //             Description = t.Description,
    //             Done = t.Done,
    //             DateCreation = t.DateCreation,
    //         })
    //         .ToListAsync(cancellationToken);
    // }
}
