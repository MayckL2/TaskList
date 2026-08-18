using Bogus;
using TaskList.DTOs;
using System.Reflection;
using Xunit;
using Xunit.Abstractions; 
using FluentAssertions;
using TaskList.Repositories;
using NSubstitute;
using Microsoft.EntityFrameworkCore.InMemory;
using TaskList.Contexts;
using Microsoft.EntityFrameworkCore;

namespace TaskList.Tests.UnitTest;
public class Database
{
    private readonly Faker _faker = new("pt_BR");
    private readonly ITaskRepository _handler;
    public Database(ITestOutputHelper output)
    {
        _handler = Substitute.For<ITaskRepository>();
    }
    
    [Fact]
    [Trait("Category", "db")]
    public async Task AfterCreatedNewTask_ShouldBeSavedOnDatabase()
    {
        // Arrange
        CreateTaskDTO dto = new CreateTaskDTO{Title = "title", Description = "description"};
        var expectedTask = new ShowTaskDTO
        {
            Id = 1,
            Title = dto.Title,
            Description = dto.Description,
            Done = false,
            DateCreation = DateTime.UtcNow,
            DateEdition = DateTime.UtcNow
        };

        _handler.CreateAsync(Arg.Any<CreateTaskDTO>())
            .Returns(Task.FromResult(expectedTask));
        
        // Act
        var result = await _handler.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(dto.Title);
        result.Description.Should().Be(dto.Description);

        await _handler.Received(1).CreateAsync(Arg.Any<CreateTaskDTO>());
    }

    [Fact]
    [Trait("Category", "db")]
    public async Task AfterGetAllTasks_ShouldNotBeNull()
    {
        // Arrange
        var expectedTasks = new List<ShowTaskDTO>
        {
            new ShowTaskDTO { Id = 1, Title = "Task 1", Description = "Task 1 Description" },
            new ShowTaskDTO { Id = 2, Title = "Task 2", Description = "Task 1 Description" }
        };

        _handler.GetAllAsync().Returns(expectedTasks.AsQueryable());

        // Act
        var result = _handler.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().Be(2);
    }
}