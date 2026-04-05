using Microsoft.AspNetCore.Identity;
using TaskList.Models;

namespace TaskList.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // 🔥 Criar roles padrão
        string[] roles = { "User", "Admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"✅ Role '{role}' criada com sucesso!");
            }
        }

        // 🔥 Criar usuário Admin padrão (opcional)
        var adminEmail = "admin@tasklist.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrador",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine($"✅ Usuário Admin criado com sucesso!");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"❌ Erro ao criar Admin: {error.Description}");
                }
            }
        }

        Console.WriteLine("🎉 Seed de dados concluído!");
    }
}
