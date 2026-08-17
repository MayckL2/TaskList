// // Fixtures/WebApplicationFactoryFixture.cs
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using TaskList.Data;
// using TaskList.Models;
// using TaskList.Contexts;
// using Microsoft.Extensions.DependencyInjection.Extensions;

// namespace TaskList.Tests.Integration.Fixtures;

// public class TestDbContext : TaskContext
// {
//     public TestDbContext(DbContextOptions<TaskContext> options) : base(options)
//     {
//     }
// }

// public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
//     where TStartup : class
// {
//     protected override void ConfigureWebHost(IWebHostBuilder builder)
//     {
//         builder.ConfigureServices(services =>
//         {
//             // 🔥 Remove a configuração do DbContext existente
//             var descriptorsToRemove = services
//                 .Where(d => d.ServiceType == typeof(DbContextOptions<TaskContext>) ||
//                            d.ServiceType == typeof(TaskContext) ||
//                            d.ServiceType == typeof(DbContext) ||
//                            d.ServiceType == typeof(IQueryable<TaskContext>) ||
//                            d.ServiceType == typeof(IDbContextFactory<TaskContext>))
//                 .ToList();

//             foreach (var descriptor in descriptorsToRemove)
//             {
//                 services.Remove(descriptor);
//             }

//             // 🔥 Adiciona DbContext em memória para testes
//             services.AddDbContext<TestDbContext >(options =>
//             {
//                 options.UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}");
//             });

//             // 🔥 3. Construir o provedor de serviços
//             var serviceProvider = services.BuildServiceProvider();

//             // 🔥 4. Criar e seedar o banco
//             using (var scope = serviceProvider.CreateScope())
//             {
//                 var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
//                 dbContext.Database.EnsureCreated();
//                 SeedData(dbContext);
//             }
//         });
//     }

//     private void SeedData(TestDbContext context)
//     {
//          // 🔥 Limpar dados existentes (evita duplicação)
//         context.Users.RemoveRange(context.Users);
//         context.SaveChanges();

//         // 🔥 Adicionar usuário de teste
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
//         context.SaveChanges();

//         // 🔥 Adicionar tarefas de teste (opcional)
//         var tasks = new List<TaskModel>
//         {
//             new TaskModel
//             {
//                 Id = 1,
//                 Title = "Task 1",
//                 Description = "Description 1",
//                 Done = false,
//                 DateCreation = DateTime.UtcNow
//             },
//             new TaskModel
//             {
//                 Id = 2,
//                 Title = "Task 2",
//                 Description = "Description 2",
//                 Done = true,
//                 DateCreation = DateTime.UtcNow
//             }
//         };

//         context.Tasks.AddRange(tasks);
//         context.SaveChanges();
//     }
// }