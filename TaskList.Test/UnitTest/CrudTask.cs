using Bogus;
using TaskList.DTOs;
using System.Reflection;
using Xunit;
using Xunit.Abstractions; 
using FluentAssertions;

namespace TaskList.Tests.UnitTest;
public class CrudTask
{
    private readonly Faker _faker = new("pt_BR");
    private readonly ITestOutputHelper _output;

    public CrudTask(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    [Trait("Approach", "Basic")]
    public void CreateTaskDTO_With_Valid_Data()
    {
        // Arrange
        var title = _faker.Lorem.Sentence(3);
        var description = _faker.Lorem.Paragraph(1);
        _output.WriteLine(title);

        // Act
        var dto = new CreateTaskDTO
        {
            Title = title,
            Description = description
        };
    
        // Assert
        Assert.Equal(title, dto.Title);
        Assert.Equal(description, dto.Description);
    }

    [Fact]
    [Trait("Approach", "FluentAssetion")]
    public void FluentAssertions_CreateTaskDTO_With_Valid_Data()
    {
        // Arrange
        var title = _faker.Lorem.Sentence(3);
        var description = _faker.Lorem.Paragraph(1);

        // Act
        var dto = new CreateTaskDTO
        {
            Title = title,
            Description = description
        };
    
        // Assert
        dto.Title.Should().Be(title);
        dto.Description.Should().Be(description);
    }

    [Theory]
    [InlineData("title", "description")]
    [InlineData("wash the dishes", "wash the dishe with a spoon")]
    [Trait("Approach", "InlineData")]
    public void FluentAssertions_CreateTaskDTO_With_Valid_Data_With_Theory(string title, string description)
    {
        // Arrange

        // Act
        var dto = new CreateTaskDTO
        {
            Title = title,
            Description = description
        };
    
        // Assert
        dto.Title.Should().Be(title);
        dto.Description.Should().Be(description);
    }
}