using AutoMapper;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using TaskList.Contexts;
using TaskList.Data;
using TaskList.GraphQL;
using TaskList.Middlewares;
using TaskList.Models;
using TaskList.Repositories;
using TaskList.Services;

var builder = WebApplication.CreateBuilder(args);

// Serilog for logs configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Lê do appsettings.json
    .Enrich.FromLogContext() // Adiciona contexto (ex: CorrelationId)
    .Enrich.WithMachineName() // Adiciona nome da máquina
    .Enrich.WithThreadId() // Adiciona ID da thread
    .WriteTo.Console() // Log no console
    .WriteTo.Seq(
        serverUrl: "http://localhost:5341", // URL do Seq
        apiKey: null // Opcional: chave de API para autenticação
    // controlLevelSwitch: null // Opcional: para mudar nível em tempo real
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
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy(
//         "AllowFrontend",
//         policy =>
//         {
//             policy
//                 .WithOrigins("http://localhost:3000", "https://seusite.com")
//                 .AllowAnyHeader()
//                 .AllowAnyMethod()
//                 .AllowCredentials();
//         }
//     );
// });

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

// Health Check
builder
    .Services.AddHealthChecks()
    .AddDbContextCheck<TaskContext>("database")
    .AddCheck<IHealthCheck>("api");

// Correction for reset password to work
// Configuring data protection between requisitions(instances) for password reset token to work
builder
    .Services.AddDataProtection()
    .PersistKeysToStackExchangeRedis(
        ConnectionMultiplexer.Connect("localhost:6379"),
        "DataProtection-Keys"
    )
    .SetApplicationName("TaskListAPI");

// 🔥 Adding GraphQL to the server
builder
    .Services.AddGraphQLServer()
    .AddQueryType<TaskQuery>()
    .AddMutationType<TaskMutation>()
    .AddSubscriptionType<TaskSubscription>()
    .AddInMemorySubscriptions();

// .AddSocketSessionInterceptor<CustomSocketInterceptor>(); // 👈 Armazenamento em memória
// .AddFiltering() // 👈 Suporte a filtros (opcional)
// .AddSorting() // 👈 Suporte a ordenação (opcional)
// .AddProjections(); // 👈 Suporte a projeções (opcional)

var app = builder.Build();

// 🔥 Middleware for logs requisitions
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
app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();
app.MapGraphQL();
app.MapControllers();
app.MapGet("/ping", () => "pong");

app.UseSerilogRequestLogging();

try
{
    Log.Information("🚀 Aplicação iniciada com sucesso");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Falha crítica na inicialização");
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
            // Verificações adicionais (memória, disco, etc.)
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
