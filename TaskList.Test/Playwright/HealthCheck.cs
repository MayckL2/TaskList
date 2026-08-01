using Microsoft.Playwright;
using Xunit;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TaskList.Tests.API;

public class HealthCheckTests
{
    private readonly string _baseUrl = "http://localhost:5245";

    [Fact]
    public async Task HealthCheck_ShouldReturn200_ShouldConnectWithDatabase_ShouldHaveMemoryOk()
    {
        // Arrange
        using var playwright = await Playwright.CreateAsync();
        var request = await playwright.APIRequest.NewContextAsync(new() { BaseURL = _baseUrl });

        // Act
        var response = await request.GetAsync("api/health/detailed");
        var responseBody = await response.JsonAsync();

        // 🔥 Usar JsonDocument para navegar no JSON
        using var document = JsonDocument.Parse(responseBody.ToString());
        var root = document.RootElement;

        var status = root.GetProperty("status").GetString();
        var databaseResult = root.GetProperty("dependencies")
            .GetProperty("database")
            .GetProperty("result")
            .GetString();

        var memoryResult = root.GetProperty("dependencies")
            .GetProperty("memory")
            .GetProperty("result")
            .GetString();

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Equal("Connected", databaseResult);
        var regex = new Regex(@"^OK\s*\(\d+\s*MB\)$");
        Assert.Matches(regex, memoryResult);
    }
}
