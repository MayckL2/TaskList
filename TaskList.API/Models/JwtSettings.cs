namespace TaskList.Models;

public class JwtSettings
{
    public string SecretKey { get; set; } = "SGk2qg1A7joXHOWqJa9n1X2JMyVaJbVXGYhaOluJOZR";
    public string Issuer { get; set; } = "TaskList";
    public string Audience { get; set; } = "User";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
