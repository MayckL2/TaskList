using System.Diagnostics;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TaskList.Contexts;

namespace TaskList.Services;

public class HealthService : IHealthService
{
    private readonly TaskContext _context;

    public HealthService(TaskContext context)
    {
        _context = context;
    }

    public async Task<string> CheckDatabase()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            // Try to connect to the database

            if (canConnect)
            {
                // Execute a real query to check database response
                await _context.Database.ExecuteSqlRawAsync("SELECT 1");
                return "Connected";
            }

            return "Disconnected";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex}: Database connection failed");
            return $"Disconnected: {ex.Message}";
        }
    }

    public async Task<string> CheckMemory()
    {
        var memory = GC.GetTotalMemory(false) / (1024 * 1024);
        return memory < 500 ? $"OK ({memory}MB)" : $"High ({memory}MB)";
    }
}
