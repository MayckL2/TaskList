using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using TaskList.DTOs;
using TaskList.Models;

namespace TaskList.GraphQL;

public class TaskSubscription
{
    // 🔥 Subscription that notify when a task is created
    [Subscribe]
    public Task<ShowTaskDTO> OnTaskCreated([EventMessage] ShowTaskDTO task) =>
        Task.FromResult(task);

    // 🔥 Subscription that notify when a task is updated
    [Subscribe]
    public Task<ShowTaskDTO> OnTaskUpdated([EventMessage] ShowTaskDTO task) =>
        Task.FromResult(task);

    // 🔥 Subscription that notify when a task is deleted
    [Subscribe]
    public Task<int> OnTaskDeleted([EventMessage] int taskId) => Task.FromResult(taskId);

    // 🔥 Subscription with filter (only concluded tasks) - NOT WORKING!!!!!
    // [Subscribe]
    // public Task<ShowTaskDTO> OnTaskCompleted([EventMessage] ShowTaskDTO task)
    // {
    //     // Só envia se a tarefa foi concluída
    //     return Task.FromResult(task);
    // }
}
