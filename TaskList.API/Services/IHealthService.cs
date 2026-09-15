namespace TaskList.Services
{
    public interface IHealthService
    {
        Task<string> CheckDatabase();
        Task<string> CheckMemory();
    }
}
