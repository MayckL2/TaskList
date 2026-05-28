using System.Net;
using System.Net.Mail;

namespace TaskList.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string email, string name)
    {
        var subject = "Bem-vindo ao TaskList!";
        var body =
            $@"
            <h1>Olá {name}!</h1>
            <p>Seu cadastro foi realizado com sucesso no TaskList.</p>
            <p>Acesse nossa plataforma e comece a organizar suas tarefas!</p>
        ";

        await SendEmailAsync(email, subject, body);
        _logger.LogInformation("Email de boas-vindas enviado para: {Email}", email);
    }

    public async Task SendPasswordResetEmailAsync(string email, string name, string resetLink)
    {
        var subject = "Recuperação de Senha - TaskList";
        var body =
            $@"
            <h1>Olá {name}!</h1>
            <p>Recebemos uma solicitação de recuperação de senha.</p>
            <p>Clique no link abaixo para redefinir sua senha:</p>
            <p><a href='{resetLink}'>Redefinir Senha</a></p>
            <p>Se você não solicitou esta alteração, ignore este email.</p>
            <p>Este link é válido por 1 hora.</p>
        ";

        await SendEmailAsync(email, subject, body);
        _logger.LogInformation("Email de recuperação enviado para: {Email}", email);
    }

    public async Task SendPasswordChangedEmailAsync(string email, string name)
    {
        var subject = "Senha Alterada - TaskList";
        var body =
            $@"
            <h1>Olá {name}!</h1>
            <p>Sua senha foi alterada com sucesso.</p>
            <p>Se você não realizou esta alteração, entre em contato com nosso suporte imediatamente.</p>
        ";

        await SendEmailAsync(email, subject, body);
        _logger.LogInformation(
            "Email de confirmação de alteração de senha enviado para: {Email}",
            email
        );
    }

    private async Task SendEmailAsync(string to, string subject, string body)
    {
        // Implementação real com SMTP, SendGrid, etc.
        // Este é um exemplo simplificado
        try
        {
            // Configurações do email
            var smtpClient = new SmtpClient(_configuration["Email:Smtp:Host"])
            {
                Port = int.Parse(_configuration["Email:Smtp:Port"]),
                Credentials = new NetworkCredential(
                    _configuration["Email:Smtp:Username"],
                    _configuration["Email:Smtp:Password"]
                ),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Email:From"]),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar email para: {Email}", to);
            throw; // Em produção, pode querer relançar ou não dependendo da necessidade
        }
    }
}
