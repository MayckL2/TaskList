using System.Diagnostics;

namespace TaskList.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. ANTES do próximo middleware
        _logger.LogInformation($"Requisição: {context.Request.Method} {context.Request.Path}");
        var stopwatch = Stopwatch.StartNew();

        // 2. Chama o PRÓXIMO middleware
        await _next(context);

        // 3. DEPOIS do próximo middleware (retorno)
        stopwatch.Stop();
        var duration = stopwatch.ElapsedMilliseconds;
        _logger.LogInformation($"Resposta: {context.Response.StatusCode} - Tempo: {duration}ms");

        if (duration > 1000)
        {
            _logger.LogWarning(
                $"⚠️ REQUISIÇÃO LENTA: {context.Request.Method} {context.Request.Path} - {duration}ms"
            );

            // Adicionar cabeçalho de aviso
            // VERIFICAR!!!
            // Metodo travando o retorno da api se demorar mais de 1 segundo
            // context.Response.Headers.Append("X-Devagar", "Essa requisição demorou mais que 1s");
        }
    }
}
