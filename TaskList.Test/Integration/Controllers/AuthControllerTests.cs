// using Microsoft.AspNetCore.Mvc.Testing;
// using System.Net.Http.Json;
// using System.Text.Json;
// using TaskList.DTOs;
// using Xunit;
// using TaskList.Tests.Fixtures;

// namespace TaskList.Tests.Integration.Controllers;

// public class AuthControllerTests : IClassFixture<TestFixture>
// {
//     private readonly HttpClient _client;
//     private readonly TestFixture _factory;

//     public AuthControllerTests(TestFixture factory)
//     {
//         _factory = factory;
//         _client = factory.Client;
//     }

//     [Fact]
//     public async Task Register_WithValidData_ShouldReturn201()
//     {
//         // Arrange
//         var registerDto = new RegisterDTO
//         {
//             FullName = "New User",
//             Email = "newuser@test.com",
//             Password = "Password@123",
//             ConfirmPassword = "Password@123"
//         };

//         // Act
//         var response = await _client.PostAsJsonAsync("/api/Auth/register", registerDto);

//         // Assert
//         Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
//         var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
//         Assert.NotNull(content);
//         Assert.True(content.Success);
//     }

//     [Fact]
//     public async Task Login_WithValidCredentials_ShouldReturnToken()
//     {
//         // Arrange
//         var loginDto = new LoginDto
//         {
//             Email = "test@email.com",
//             Password = "Password@123"
//         };

//         // Act
//         var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

//         // Assert
//         Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
//         var content = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
//         Assert.NotNull(content);
//         Assert.NotNull(content.AccessToken);
//     }

//     [Fact]
//     public async Task Login_WithInvalidCredentials_ShouldReturn401()
//     {
//         // Arrange
//         var loginDto = new LoginDto
//         {
//             Email = "test@email.com",
//             Password = "WrongPassword"
//         };

//         // Act
//         var response = await _client.PostAsJsonAsync("/api/Auth/login", loginDto);

//         // Assert
//         Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
//     }
// }