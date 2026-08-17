// Helpers/AuthHelper.cs
using System.Net.Http.Json;
using TaskList.DTOs;

namespace TaskList.Tests.Integration.Helpers;

public static class AuthHelper
{
    public static async Task<string> GetAuthTokenAsync(HttpClient client)
    {
        var loginDto = new LoginDto
        {
            Email = "test@email.com",
            Password = "Password@123"
        };

        var response = await client.PostAsJsonAsync("/api/Auth/login", loginDto);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

        return authResponse?.AccessToken ?? string.Empty;
    }

    public static void AddAuthHeader(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer{token}");
    }
}