// Na mesma pasta do seu DbContext ou em uma pasta "Factories"
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TaskList.Contexts;

public class TaskContextFactory : IDesignTimeDbContextFactory<TaskContext>
{
    public TaskContext CreateDbContext(string[] args)
    {
        // Configuração para encontrar o appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.Development.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<TaskContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);

        return new TaskContext(optionsBuilder.Options);
    }
}
