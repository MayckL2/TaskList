// // Services/TaskServiceTests.cs
// using Microsoft.Extensions.DependencyInjection;
// using TaskList.Services;
// using Xunit;
// using TaskList.DTOs;
// using TaskList.Tests.Fixtures;

// namespace TaskList.Tests.Integration.Services;

// public class TaskServiceTests : IClassFixture<TestFixture>
// {
//     private readonly IServiceProvider _serviceProvider;

//     public TaskServiceTests(TestFixture factory)
//     {
//         _serviceProvider = factory.ServiceProvider;
//     }

//     [Fact]
//     public async Task GetAllTasks_ShouldReturnTasks()
//     {
//         // Arrange
//         using var scope = _serviceProvider.CreateScope();
//         var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

//         // Act
//         var tasks = await taskService.GetAllAsync();

//         // Assert
//         Assert.NotNull(tasks);
//         // Add more assertions based on your seed data
//     }

//     [Fact]
//     public async Task CreateTask_ShouldAddTask()
//     {
//         // Arrange
//         using var scope = _serviceProvider.CreateScope();
//         var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();

//         var newTask = new CreateTaskDTO
//         {
//             Title = "Service Test Task",
//             Description = "Created via service test"
//         };

//         // Act
//         var createdTask = await taskService.CreateAsync(newTask);

//         // Assert
//         Assert.NotNull(createdTask);
//         Assert.Equal(newTask.Title, createdTask.Title);
//         Assert.False(createdTask.Done);
//         Assert.NotEqual(default, createdTask.DateCreation);
//     }
// }