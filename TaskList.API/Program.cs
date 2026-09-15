using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using StackExchange.Redis;
using TaskList.Contexts;
using TaskList.Data;
using TaskList.GraphQL;
using TaskList.Middlewares;
using TaskList.Models;
using TaskList.Repositories;
using TaskList.Services;
using TaskList.API;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    jwtSettings["SecretKey"]
                        ?? throw new InvalidOperationException(
                            "JWT SecretKey não configurada no appsettings.json"
                        )
                )
            ),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // Important for docker
        };

        // For debug
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌ Falha na autenticação: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var principal = context.Principal;

                Console.WriteLine("📋 Claims do token:");
                if (principal?.Claims != null)
                {
                    foreach (var claim in principal.Claims)
                    {
                        Console.WriteLine($"   {claim.Type}: {claim.Value}");
                    }
                    return Task.CompletedTask;
                }
                else
                {
                    Console.WriteLine("Claims não localizadas...");
                    return Task.CompletedTask;
                }
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"⚠️ Challenge: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            },
        };
    });

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Serilog for logs configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext() // Adding context (ex: CorrelationId)
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console() // Log for console
    .WriteTo.Seq(serverUrl: "http://localhost:5341", apiKey: null) // Login/Password: admin
    .CreateLogger();

builder.Logging.ClearProviders();
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
    .Services.AddIdentityCore<User>(options =>
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
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TaskContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<RoleManager<IdentityRole>>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:8080", // Swagger Docker
                    "http://localhost:5000", // Swagger local
                    "http://localhost:5001", // Swagger local HTTPS
                    "http://localhost:4200", // Angular
                    "http://localhost:3000" // React
                )
                .AllowAnyHeader()
                .AllowCredentials()
                .AllowAnyMethod();
        }
    );
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

// AutoMapper Register
builder.Services.AddAutoMapper(
    cfg =>
    {
        cfg.AllowNullDestinationValues = true;
        cfg.AllowNullCollections = true;
    },
    typeof(Program)
);

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHealthService, HealthService>();

// Health Check
builder
    .Services.AddHealthChecks()
    .AddDbContextCheck<TaskContext>("database")
    .AddCheck<IHealthCheck>("api");

// HANGFIRE
builder.Services.AddHangfire(configuration =>
    configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
            }
        )
);

// HANGFIRE (processing jobs)
builder.Services.AddHangfireServer();

builder.Services.AddScoped<IHangFire, HangFire>();

// GraphQL
builder
    .Services.AddGraphQLServer()
    .AddQueryType<TaskQuery>()
    .AddMutationType<TaskMutation>()
    .AddSubscriptionType<TaskSubscription>()
    .AddInMemorySubscriptions();

var app = builder.Build();

// DASHBOARD (monitoring interface)
app.UseHangfireDashboard(
    "/hangfire",
    new DashboardOptions
    {
        Authorization = new[]
        {
            new BasicAuthAuthorizationFilter(
                new BasicAuthAuthorizationFilterOptions
                {
                    RequireSsl = false,
                    SslRedirect = false,
                    LoginCaseSensitive = true,
                    Users = new[]
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login = "admin",
                            PasswordClear = "Hangfire@123",
                        },
                    },
                }
            ),
        },
    }
);

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
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("TaskList .NET 10")
            .WithTheme(ScalarTheme.DeepSpace) // Escolha seu tema visual
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
            .AddPreferredSecuritySchemes("BearerAuth");
    });

    using var scope = app.Services.CreateScope();
    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

    // Validate all mapping
    try
    {
        mapper.ConfigurationProvider.AssertConfigurationIsValid();
        Console.WriteLine("Configuração do AutoMapper valida!");
    }
    catch (AutoMapperConfigurationException ex)
    {
        Console.WriteLine($"Erro na configuração do AutoMapper: {ex.Message}");
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

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();
app.MapGraphQL();
app.MapControllers();

// JOBS (opcional: agendar ao iniciar)
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

// Recurring task: clean logs every day at 3.
recurringJobManager.AddOrUpdate<IHangFire>(
    "limpar-logs",
    service => service.LimparLogsAsync(),
    Cron.Daily(3)
);

// Recurring task: checking overdue tasks every hour
recurringJobManager.AddOrUpdate<IHangFire>(
    "verificar-tarefas-atrasadas",
    service => service.VerificarTarefasAtrasadasAsync(),
    Cron.Minutely()
);

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