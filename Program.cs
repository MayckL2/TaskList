using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskList.Contexts;
using TaskList.IServices;
using TaskList.Middlewares;
using TaskList.Repositories;
using TaskList.Services;
using TaskList.Mapping;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TaskContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AutoMapper Register
builder.Services.AddAutoMapper(typeof(TaskProfile));

// TaskRepository scoped
builder.Services.AddScoped<TaskRepository>();

// Taskservice scoped
builder.Services.AddScoped<ITaskService, TaskService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

    // Validar todos os mapeamentos
    try
    {
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
        Console.WriteLine("✅ Configuração do AutoMapper é válida!");
    }
    catch (AutoMapperConfigurationException ex)
    {
        Console.WriteLine($"❌ Erro na configuração do AutoMapper: {ex.Message}");
        // Log detalhado dos erros
        foreach (var error in ex.Errors)
        {
            Console.WriteLine(
                $"  - {error.TypeMap.SourceType.Name} -> {error.TypeMap.DestinationType.Name}"
            );
            foreach (var unmappedProperty in error.UnmappedPropertyNames)
            {
                Console.WriteLine($"    Propriedade não mapeada: {unmappedProperty}");
            }
        }
    }
}

app.UseHttpsRedirection();

// Adding midlewares
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
