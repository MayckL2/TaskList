using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.SqlServer;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");

builder.Services.AddAuthentication(options =>
{
    // 🔥 FORÇAR O JWT COMO ESQUEMA PADRÃO
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
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada no appsettings.json"))),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // 🔥 IMPORTANTE para Docker
        };
        
        // 🔥 EVENTOS PARA DEBUG
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
                
                // Console.WriteLine($"✅ Token validado: {context.Principal?.Identity?.Name}");
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"⚠️ Challenge: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            }
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

// CORS - ######## Restrito para origens expecificas #########
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:8080",    // Swagger Docker
                    "http://localhost:5000",    // Swagger local
                    "http://localhost:5001",    // Swagger local HTTPS
                    "http://localhost:4200",    // Angular
                    "http://localhost:3000"     // React
                )
                // .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowCredentials()
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
builder.Services.AddAutoMapper(
    cfg =>
    {
        cfg.AllowNullDestinationValues = true;
        cfg.AllowNullCollections = true;
    },
    typeof(Program)
);

// TaskRepository scoped
builder.Services.AddScoped<TaskRepository>();

// Taskservice scoped
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHealthService, HealthService>();

// Health Check
builder
    .Services.AddHealthChecks()
    .AddDbContextCheck<TaskContext>("database")
    .AddCheck<IHealthCheck>("api");

// 🔥 1. ADICIONAR HANGFIRE
builder.Services.AddHangfire(configuration => configuration
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
            DisableGlobalLocks = true
        }));

// 🔥 2. ADICIONAR O SERVIDOR HANGFIRE (processa os jobs)
builder.Services.AddHangfireServer();

builder.Services.AddScoped<IHangFire, HangFire>();

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

// 🔥 3. DASHBOARD (interface de monitoramento)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
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

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();
app.MapGraphQL();
app.MapControllers();

// 🔥 4. EXEMPLO DE JOBS (opcional: agendar ao iniciar)
var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

// 🔥 Job recorrente: limpar logs todo dia às 3h
recurringJobManager.AddOrUpdate<IHangFire>(
    "limpar-logs",
    service => service.LimparLogsAsync(),
    Cron.Daily(3)
);

// 🔥 Job recorrente: verificar tarefas atrasadas a cada hora
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
