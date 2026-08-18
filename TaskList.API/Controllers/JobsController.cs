using Hangfire;
using Microsoft.AspNetCore.Mvc;
using TaskList.Services;

namespace TaskList.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ITaskService _taskService;
    private readonly IHangFire _hangfire;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        ITaskService taskService,
        IHangFire hangfire,
        ILogger<JobsController> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _taskService = taskService;
        _hangfire = hangfire;
        _logger = logger;
    }

    // 🔥 Enfileirar job para execução imediata
    [HttpPost("processar-pedido/{id}")]
    public async Task<IActionResult> ProcessarPedido(int id)
    {
        _backgroundJobClient.Enqueue<IHangFire>(
            service => service.ProcessarPedidoAsync(id));

        _logger.LogInformation($"📦 Pedido{id} enfileirado para processamento");
        return Accepted(new { message = "Pedido enfileirado para processamento" });
    }

    // 🔥 Agendar job para execução futura
    [HttpPost("enviar-notificacao")]
    public async Task<IActionResult> EnviarNotificacao(string userId, string mensagem, int delayMinutes = 5)
    {
        _backgroundJobClient.Schedule<IHangFire>(
            service => service.EnviarNotificacaoAsync(userId, mensagem),
            TimeSpan.FromMinutes(delayMinutes));

        _logger.LogInformation($"📨 Notificação agendada para{userId} em{delayMinutes} minutos");
        return Accepted(new { message = $"Notificação agendada para{delayMinutes} minutos" });
    }

    // 🔥 Executar job manualmente (on-demand)
    [HttpPost("limpar-logs")]
    public async Task<IActionResult> LimparLogs()
    {
        _backgroundJobClient.Enqueue<IHangFire>(
            service => service.LimparLogsAsync());

        return Accepted(new { message = "Limpeza de logs iniciada" });
    }

    // 🔥 Executar job manualmente (on-demand com resultado)
    [HttpPost("gerar-relatorio")]
    public async Task<IActionResult> GerarRelatorio()
    {
        var jobId = _backgroundJobClient.Enqueue<IHangFire>(
            service => service.GerarRelatorioDiarioAsync());

        return Accepted(new
        {
            message = "Geração de relatório iniciada",
            jobId = jobId
        });
    }

    // 🔥 Agendar job recorrente via API
    [HttpPost("agendar")]
    public async Task<IActionResult> AgendarJob(string jobId, string cronExpression)
    {
        _recurringJobManager.AddOrUpdate<IHangFire>(
            jobId,
            service => service.VerificarTarefasAtrasadasAsync(),
            cronExpression);

        _logger.LogInformation($"⏰ Job{jobId} agendado com cron '{cronExpression}'");
        return Ok(new { message = $"Job{jobId} agendado com sucesso" });
    }

    // 🔥 Remover job recorrente
    [HttpDelete("agendar/{jobId}")]
    public async Task<IActionResult> RemoverJob(string jobId)
    {
        _recurringJobManager.RemoveIfExists(jobId);
        _logger.LogInformation($"🗑️ Job{jobId} removido");
        return Ok(new { message = $"Job{jobId} removido" });
    }
}