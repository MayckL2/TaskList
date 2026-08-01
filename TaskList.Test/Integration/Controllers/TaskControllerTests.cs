using System.Net.Http.Json;
using System.Text.Json;
using TaskList.DTOs;
using TaskList.Tests.Integration.Helpers;
using Xunit;
using TaskList.Tests.Fixtures;

namespace TaskList.Tests.Integration.Controllers;

public class TaskControllerTests : IClassFixture<TestFixture>
{
    private readonly HttpClient _client;
    private readonly TestFixture _factory;
    private readonly string _authToken;

    public TaskControllerTests(TestFixture factory)
    {
        _factory = factory;
        _client = factory.Client;

        // 🔥 Obter token para testes autenticados
        _authToken = AuthHelper.GetAuthTokenAsync(_client).Result;
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer{_authToken}");
    }

    [Fact]
    public async Task GetAllTasks_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/Task");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<IEnumerable<ShowTaskDTO>>();
        Assert.NotNull(tasks);
    }

    [Fact]
    public async Task CreateTask_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var newTask = new CreateTaskDTO
        {
            Title = "Test Task",
            Description = "Created via integration test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Task", newTask);
        var createdTask = await response.Content.ReadFromJsonAsync<ShowTaskDTO>();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(newTask.Title, createdTask?.Title);
    }

    [Fact]
    public async Task UpdateTask_WithValidData_ShouldReturnOk()
    {
        // Arrange - criar uma tarefa primeiro
        var newTask = new CreateTaskDTO
        {
            Title = "Task to Update",
            Description = "Will be updated"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Task", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<ShowTaskDTO>();

        // Arrange - dados para atualização
        var updateDto = new UpdateTaskDTO
        {
            Title = "Updated Task",
            Description = "This was updated",
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Task/{createdTask!.Id}", updateDto);
        var updatedTask = await response.Content.ReadFromJsonAsync<ShowTaskDTO>();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(updateDto.Title, updatedTask?.Title);
        Assert.Equal(updateDto.Description, updatedTask?.Description);
        Assert.True(updatedTask?.Done);
    }

    [Fact]
    public async Task DeleteTask_ShouldReturnNoContent()
    {
        // Arrange - criar uma tarefa
        var newTask = new CreateTaskDTO
        {
            Title = "Task to Delete",
            Description = "Will be deleted"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Task", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<ShowTaskDTO>();

        // Act
        var response = await _client.DeleteAsync($"/api/Task/{createdTask!.Id}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);

        // Verificar se realmente foi deletada
        var getResponse = await _client.GetAsync($"/api/Task/{createdTask.Id}");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}