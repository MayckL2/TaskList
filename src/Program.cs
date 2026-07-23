using AutoMapper;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskList.Contexts;
using TaskList.Data;
using TaskList.Middlewares;
using TaskList.Models;
using TaskList.Repositories;
using TaskList.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:80");

// OU via Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80);
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Serilog for logs configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // LÃª do appsettings.json
    .Enrich.FromLogContext() // Adiciona contexto (ex: CorrelationId)
    .Enrich.WithMachineName() // Adiciona nome da mÃ¡quina
    .Enrich.WithThreadId() // Adiciona ID da thread
    .WriteTo.Console() // Log no console
    .WriteTo.Seq(
        serverUrl: "http://localhost:5341", // URL do Seq
        apiKey: null // Opcional: chave de API para autenticaÃ§Ã£o
    // controlLevelSwitch: null // Opcional: para mudar nÃ­vel em tempo real
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Add redis chache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "TaskList";
});

// Add services to the container.
builder.Services.AddDbContext<TaskContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Identity
builder
    .Services.AddIdentity<User, IdentityRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<TaskContext>()
    .AddDefaultTokenProviders();

// CORS - ######## Restrito para origens expecificas #########
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy
                // .WithOrigins("http://localhost:3000", "https://seusite.com")
                .AllowAnyOrigin()
                .AllowAnyHeader()
                // .AllowCredentials()
                .AllowAnyMethod();
        }
    );
});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TaskList API", Version = "v1" });

    c.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        }
    );
});

// AutoMapper Register
builder.Services.AddAutoMapper(typeof(Program));

// TaskRepository scoped
builder.Services.AddScoped<TaskRepository>();

// Taskservice scoped
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHealthService, HealthService>();

// Health Check
builder
    .Services.AddHealthChecks()
    .AddDbContextCheck<TaskContext>("database")
    .AddCheck<IHealthCheck>("api");

// Correction for reset password to work
// Configuring data protection between requisitions(instances) for password reset token to work
// var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "redis:6379";

builder
    .Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(
        ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "redis:6379,abortConnect=false"),
        "DataProtection-Keys"
    )
    .SetApplicationName("TaskListAPI");

var app = builder.Build();

// Middleware for logs requisitions
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("User", httpContext.User.Identity?.Name);
        diagnosticContext.Set("RemoteIp", httpContext.Connection.RemoteIpAddress?.ToString());
    };
});

using (var scope = app.Services.CreateScope())
{
     var dbContext = scope.ServiceProvider.GetRequiredService<TaskContext>();
    
    try
    {
        Console.WriteLine("🔄 Criando banco de dados...");
        await dbContext.Database.EnsureCreatedAsync();
        Console.WriteLine("✅ Banco criado com sucesso!");
        
        Console.WriteLine("🔄 Aplicando migrações...");
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("✅ Migrações aplicadas!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro: {ex.Message}");
    }

    var services = scope.ServiceProvider;
    try
    {
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao executar o seeder de dados.");
    }
}

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
        Console.WriteLine("Configuração do AutoMapper valida!");
    }
    catch (AutoMapperConfigurationException ex)
    {
        Console.WriteLine($"Erro na configuração do AutoMapper: {ex.Message}");
        // Log detalhado dos erros
        foreach (var error in ex.Errors)
        {
            Console.WriteLine(
                $"  - {error.TypeMap.SourceType.Name} -> {error.TypeMap.DestinationType.Name}"
            );
            foreach (var unmappedProperty in error.UnmappedPropertyNames)
            {
                Console.WriteLine($"Propriedade não mapeada: {unmappedProperty}");
            }
        }
    }
}

app.UseHttpsRedirection();

// Adding midlewares
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/ping", () => "pong");

app.UseSerilogRequestLogging();

try
{
    Log.Information("Aplicão iniciada com sucesso");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Falha critica na inicialização");
}
finally
{
    Log.CloseAndFlush();
}

public class ApiHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var isHealthy = true;

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("API is healthy"));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("API is unhealthy"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(ex.Message));
        }
    }
}


