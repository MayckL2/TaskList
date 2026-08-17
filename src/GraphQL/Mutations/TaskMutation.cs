using System.Data.Common;
using HotChocolate;
using HotChocolate.Subscriptions;
using TaskList.DTOs;
using TaskList.Models;
using TaskList.Repositories;
using TaskList.Services;

namespace TaskList.GraphQL;

public class TaskMutation
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TaskMutation> _logger;
    private readonly ITopicEventSender _eventSender;

    public TaskMutation(
        ITaskService taskService,
        ILogger<TaskMutation> logger,
        ITopicEventSender eventSender
    )
    {
        _logger = logger;
        _taskService = taskService;
        _eventSender = eventSender;
    }

    // 🔥 Mutation: Create task
    public async Task<ShowTaskDTO> CreateTaskAsync(
        CreateTaskDTO input,
        CancellationToken cancellationToken
    )
    {
        var result = await _taskService.CreateAsync(input);

        // 🔥 Publicar evento para todos os subscribers
        await _eventSender.SendAsync(
            nameof(TaskSubscription.OnTaskCreated),
            result,
            cancellationToken
        );

        // 🔥 Se for concluída, publicar também no tópico específico
        if (result.Done)
        {
            await _eventSender.SendAsync(
                nameof(TaskSubscription.OnTaskCreated),
                result,
                cancellationToken
            );
        }

        return result;
    }

    // 🔥 Mutation: Update task
    public async Task<ShowTaskDTO> UpdateTaskAsync(
        UpdateTaskDTO input,
        CancellationToken cancellationToken
    )
    {
        var update = await _taskService.UpdateAsync(input);

        Console.WriteLine(update);
        await _eventSender.SendAsync("TaskEvents", update, cancellationToken);

        return update;
    }

    // 🔥 Mutation: Delete task
    public async Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken)
    {
        var delete = await _taskService.DeleteAsync(id);

        _logger.LogInformation("🗑️ Cache da lista invalidado");

        if (delete)
        {
            await _eventSender.SendAsync("TaskEvents", delete, cancellationToken);
            return true;
        }
        else
        {
            throw new GraphQLException("Tarefa não encontrada");
        }
    }
}
