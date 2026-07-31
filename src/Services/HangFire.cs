using Hangfire;
using TaskList.Models;
using TaskList.Contexts;
using Microsoft.EntityFrameworkCore; 

namespace TaskList.Services;

public class HangFire : IHangFire
{
    private readonly TaskContext _context;
    private readonly ILogger<HangFire> _logger;
    private readonly IEmailService _emailService;

    public HangFire(TaskContext context, ILogger<HangFire> logger, IEmailService emailService)
    {
        _context = context;
        _logger = logger;
        _emailService = emailService;
    }

    // 🔥 JOB 1: Limpar logs antigos
    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task LimparLogsAsync()
    {
        _logger.LogInformation("🔃 Iniciando limpeza de logs...");

        // var dataLimite = DateTime.UtcNow.AddDays(-30);
        // var logsAntigos = _context.Logs.Where(l => l.Data < dataLimite);

        // var count = logsAntigos.Count();
        // if (count > 0)
        // {
        //     _context.Logs.RemoveRange(logsAntigos);
        //     await _context.SaveChangesAsync();
        //     _logger.LogInformation($"✅{count} logs removidos");
        // }
        // else
        // {
        //     _logger.LogInformation("✅ Nenhum log antigo para remover");
        // }
    }

    // 🔥 JOB 2: Verificar tarefas atrasadas
    [AutomaticRetry(Attempts = 5, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task VerificarTarefasAtrasadasAsync()
    {
        _logger.LogInformation("🔍 Verificando tarefas atrasadas...");

        var tarefasAtrasadas = await _context.Tasks
            .Where(t => !t.Done)
            .ToListAsync();

        if (tarefasAtrasadas.Any())
        {
            _logger.LogWarning($"⚠️{tarefasAtrasadas.Count} tarefas atrasadas encontradas");

            // foreach (var tarefa in tarefasAtrasadas)
            // {
            //     // 🔥 Notificar responsável
            //     await EnviarNotificacaoAsync(tarefa.Id.ToString(), $"Tarefa '{tarefa.Title}' está atrasada!");
            // }

        }
        else
        {
            _logger.LogInformation("✅ Nenhuma tarefa atrasada");
        }
    }

    // 🔥 JOB 3: Enviar notificação
    public async Task EnviarNotificacaoAsync(string userId, string mensagem)
    {
        _logger.LogInformation($"📨 Enviando notificação para usuário{userId}");

        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            // await _emailService.SendEmailAsync(user.Email, "Notificação", mensagem);
            _logger.LogInformation($"✅ Notificação enviada para{user.Email}");
        }
    }

    // 🔥 JOB 4: Processar pedido (simulação)
    public async Task ProcessarPedidoAsync(int pedidoId)
    {
        _logger.LogInformation($"📦 Processando pedido{pedidoId}");

        // Simula processamento longo
        // await Task.Delay(5000);

        // var pedido = await _context.Pedidos.FindAsync(pedidoId);
        // if (pedido != null)
        // {
        //     pedido.Status = "Processado";
        //     pedido.DataProcessamento = DateTime.UtcNow;
        //     await _context.SaveChangesAsync();

        //     // 🔥 Agendar job para enviar confirmação
        //     BackgroundJob.Schedule<IHangFire>(
        //         service => service.EnviarNotificacaoAsync(
        //             pedido.UsuarioId,
        //             $"Pedido{pedidoId} processado com sucesso!"),
        //         TimeSpan.FromMinutes(1));

        //     _logger.LogInformation($"✅ Pedido{pedidoId} processado");
        // }
    }

    // 🔥 JOB 5: Gerar relatório diário
    public async Task GerarRelatorioDiarioAsync()
    {
        _logger.LogInformation("📊 Gerando relatório diário...");

        var hoje = DateTime.UtcNow.Date;
        var tarefasHoje = await _context.Tasks
            .Where(t => t.DateCreation.Date == hoje)
            .CountAsync();

        var tarefasConcluidasHoje = await _context.Tasks
            .Where(t => t.Done && t.DateEdition.Date == hoje)
            .CountAsync();

        var relatorio = new
        {
            Date = hoje,
            CreatedTasks = tarefasHoje,
            DoneTasks = tarefasConcluidasHoje,
            TotalTasks = await _context.Tasks.CountAsync<TaskModel>()
        };

        // 🔥 Salvar relatório
        _context.Reports.Add(new Report
        {
            Date = hoje,
            Data = System.Text.Json.JsonSerializer.Serialize(relatorio)
        });
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Relatório diário gerado:{tarefasHoje} tarefas criadas,{tarefasConcluidasHoje} concluídas");
    }
}