namespace TaskList.Services;

public interface IHangFire
{
    Task LimparLogsAsync();
    Task VerificarTarefasAtrasadasAsync();
    Task EnviarNotificacaoAsync(string userId, string mensagem);
    Task ProcessarPedidoAsync(int pedidoId);
    Task GerarRelatorioDiarioAsync();
}