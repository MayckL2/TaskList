using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TaskList.DTOs;

namespace TaskList.Hubs;

// Still to implement authorization
// [Authorize]
public class TaskHub : Hub
{
    private readonly ILogger<TaskHub> _logger;

    public TaskHub(ILogger<TaskHub> logger)
    {
        _logger = logger;
    }

    // 🔥 Method to client join group
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation($"Cliente {Context.ConnectionId} entrou no grupo {groupName}");

        await Clients.Caller.SendAsync("Notification", $"Você entrou no grupo {groupName}");
    }

    // 🔥 Method to client leave the group
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation($"Cliente {Context.ConnectionId} saiu do grupo {groupName}");
    }

    // 🔥 Method to send message to the client
    public async Task SendNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }

    // 🔥 Method to send notification for a especifc group
    public async Task SendNotificationToGroup(string groupName, string message)
    {
        await Clients.Group(groupName).SendAsync("ReceiveNotification", message);
    }

    // 🔥 Method when a new task is created (broadcast)
    public async Task BroadcastNewTask(ShowTaskDTO task)
    {
        await Clients.All.SendAsync("TaskCreated", task);
        _logger.LogInformation($"Tarefa {task.Id} criada e broadcast enviado");
    }

    // 🔥 Event for connection/disconnection
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation($"Cliente conectado: {Context.ConnectionId}");
        await Clients.Caller.SendAsync("Notification", "Bem-vindo ao sistema de tarefas!");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation($"Cliente desconectado: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }
}
