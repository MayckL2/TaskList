using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using TaskList.DTOs;
using TaskList.Models;

namespace TaskList.GraphQL;

public class TaskSubscription
{
    // 🔥 Subscription que notifica quando uma tarefa é criada
    [Subscribe]
    public Task<ShowTaskDTO> OnTaskCreated([EventMessage] ShowTaskDTO task) =>
        Task.FromResult(task);

    // 🔥 Subscription que notifica quando uma tarefa é atualizada
    [Subscribe]
    public Task<ShowTaskDTO> OnTaskUpdated([EventMessage] ShowTaskDTO task) =>
        Task.FromResult(task);

    // 🔥 Subscription que notifica quando uma tarefa é deletada
    [Subscribe]
    public Task<int> OnTaskDeleted([EventMessage] int taskId) => Task.FromResult(taskId);

    // 🔥 Subscription com filtro (apenas tarefas concluídas)
    [Subscribe]
    public Task<ShowTaskDTO> OnTaskCompleted([EventMessage] ShowTaskDTO task)
    {
        // Só envia se a tarefa foi concluída
        return Task.FromResult(task);
    }
}
