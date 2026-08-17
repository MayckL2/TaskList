// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using TaskList.Contexts;
// using TaskList.Models;
// using TaskList.Services; // 👈 Adicione
// using TaskList.Repositories; // 👈 Adicione
// using TaskList.Mapping;
// using Microsoft.Extensions.DependencyInjection.Extensions;

// namespace TaskList.Tests.Fixtures;

// public class TestFixture : IDisposable
// {
//     public HttpClient Client { get; }
//     public TaskContext Context { get; }
//     public IServiceProvider ServiceProvider { get; }

//     public TestFixture()
//     {
//         // 🔥 1. Criar fábrica com banco mockado
//         var factory = new WebApplicationFactory<Program>()
//             .WithWebHostBuilder(builder =>
//             {
//                 builder.ConfigureServices(services =>
//                 {
//                     // 🔥 Remover DbContext existente
//                     var descriptors = services
//                         .Where(d => d.ServiceType == typeof(DbContextOptions<TaskContext>) ||
//                                    d.ServiceType == typeof(TaskContext))
//                         .ToList();

//                     foreach (var descriptor in descriptors)
//                     {
//                         services.Remove(descriptor);
//                     }

//                     services.RemoveAll<DbContextOptions<TaskContext>>();
//                     services.RemoveAll<TaskContext>();

//                     // 🔥 Adicionar banco em memória
//                     services.AddDbContext<TaskContext>(options =>
//                     {
//                         options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
//                         options.EnableSensitiveDataLogging();
//                     });

//                     // 🔥 3. REGISTRAR OS SERVIÇOS QUE VOCÊ VAI TESTAR
//                     services.AddScoped<ITaskService, TaskService>();
//                     services.AddScoped<IUserRepository, UserRepository>();
//                     services.AddScoped<IAuthService, AuthService>();
//                     services.AddAutoMapper(typeof(TaskProfile)); // Se usar AutoMapper

//                     // 🔥 Construir e seedar
//                     var serviceProvider = services.BuildServiceProvider();
//                     using var scope = serviceProvider.CreateScope();
//                     var dbContext = scope.ServiceProvider.GetRequiredService<TaskContext>();
//                     dbContext.Database.EnsureCreated();
//                     SeedData(dbContext);
//                 });
//             });

//         Client = factory.CreateClient();
//         ServiceProvider = factory.Services;

//         using var scope = ServiceProvider.CreateScope();
//         Context = scope.ServiceProvider.GetRequiredService<TaskContext>();
//     }

//     private void SeedData(TaskContext context)
//     {
//         context.Database.EnsureCreated();

//         // 🔥 Usuário de teste
//         var user = new User
//         {
//             Id = "test-user-id",
//             UserName = "test@email.com",
//             Email = "test@email.com",
//             FullName = "Test User",
//             IsActive = true,
//             EmailConfirmed = true
//         };

//         context.Users.Add(user);

//         // 🔥 Tarefas de teste
//         context.Tasks.AddRange(new[]
//         {
//             new TaskModel
//             {
//                 Id = 1,
//                 Title = "Test Task 1",
//                 Description = "Description 1",
//                 Done = false,
//                 DateCreation = DateTime.UtcNow
//             },
//             new TaskModel
//             {
//                 Id = 2,
//                 Title = "Test Task 2",
//                 Description = "Description 2",
//                 Done = true,
//                 DateCreation = DateTime.UtcNow
//             }
//         });

//         context.SaveChanges();
//     }

//     public void Dispose()
//     {
//         Client.Dispose();
//         Context.Dispose();
//     }
// }