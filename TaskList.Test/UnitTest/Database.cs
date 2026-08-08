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
    private readonly ITestOutputHelper _output;
    private readonly ITaskRepository _handler;
    private readonly TaskContext _appDb;

    public Database(ITestOutputHelper output)
    {
        _output = output;

        var dbContextOptions = new DbContextOptionsBuilder<TaskContext>();
        dbContextOptions.UseInMemoryDatabase("TaskContext");

        _appDb = new TaskContext(dbContextOptions.Options);

        _handler = Substitute.For<ITaskRepository>();
    }
    
    [Fact]
    [Trait("Category", "db")]
    public async void AfterCreatedNewTask_ShouldBeSavedOnDatabase()
    {
        // Arrange
        CreateTaskDTO dto = new CreateTaskDTO{Title = "title", Description = "description"};

        // Act
        // var result = await _appDb.Tasks.SingleOrDefaultAsync(x => x.Id == id);
        var result = await _handler.CreateAsync(dto);
        // _output.WriteLine(result.Id.ToString());

        // Assert
        result.Title.Should().Be(dto.Title);

    }
}