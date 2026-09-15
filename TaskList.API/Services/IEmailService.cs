namespace TaskList.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string name);
    Task SendPasswordResetEmailAsync(string email, string name, string resetLink);
    Task SendPasswordChangedEmailAsync(string email, string name);
}
