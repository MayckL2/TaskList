// using System.Text.Json;
// using Microsoft.Playwright;

// public class AuthTests
// {
//     private readonly string _baseUrl = "http://localhost:5245";

//     [Fact]
//     public async Task Login_WithValidCredentials_ShouldReturnToken()
//     {
//         // Arrange
//         using var playwright = await Playwright.CreateAsync();
//         var request = await playwright.APIRequest.NewContextAsync(new()
//         {
//             BaseURL = _baseUrl
//         });

//         var loginData = new
//         {
//             email = "admin@tasklist.com",
//             password = "Admin@123",
//             rememberMe = false
//         };

//         var options = new APIRequestContextOptions
//         {
//             Data = loginData
//         };

//         // Act
//         var response = await request.PostAsync("/api/Auth/login", options);

//         // Assert
//         Assert.Equal(200, response.Status);

//         var responseBody = await response.JsonAsync();
//         var token = responseBody?.GetProperty("accessToken").GetString();
//         Assert.NotNull(token);
//     }

//     [Fact]
//     public async Task Login_WithInvalidCredentials_ShouldReturn401()
//     {
//         // Arrange
//         using var playwright = await Playwright.CreateAsync();
//         var request = await playwright.APIRequest.NewContextAsync(new()
//         {
//             BaseURL = _baseUrl
//         });

//         var loginData = new
//         {
//             email = "admin@tasklist.com",
//             password = "SenhaErrada"
//         };

//         // Act
//         var response = await request.PostAsync("/api/Auth/login", new()
//         {
//             Data = loginData
//         });

//         // Assert
//         Assert.Equal(401, response.Status);
//     }
// }