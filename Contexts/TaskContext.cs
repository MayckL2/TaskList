using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskList.Models;

namespace TaskList.Contexts;

public class TaskContext : IdentityDbContext<User>
{
    public TaskContext(DbContextOptions<TaskContext> options)
        : base(options) { }

    public DbSet<TaskModel> Tasks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    // specify the id as the primary key in the Model

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskModel>().HasKey(t => t.Id);
        // Configuração da tabela Users (já configurada pelo Identity)

        // ✅ CORRETO: usar 'modelBuilder' em vez de 'builder'
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();

            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(u => u.Email).IsUnique();
        });

        // ✅ CORRETO: usar 'modelBuilder'
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);

            entity.Property(rt => rt.Token).HasMaxLength(500).IsRequired();

            entity.HasIndex(rt => rt.Token).IsUnique();

            entity.Property(rt => rt.ExpiresAt).IsRequired();

            entity.Property(rt => rt.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Relacionamento com User
            entity
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });
        });
        // Configuração da tabela UserClaims (customizada)
        // builder.Entity<UserClaim>(entity =>
        // {
        //     entity.HasKey(uc => uc.Id);

        //     entity.Property(uc => uc.ClaimType).HasMaxLength(100).IsRequired();

        //     entity.Property(uc => uc.ClaimValue).HasMaxLength(100).IsRequired();

        //     entity
        //         .HasIndex(uc => new
        //         {
        //             uc.UserId,
        //             uc.ClaimType,
        //             uc.ClaimValue,
        //         })
        //         .IsUnique();

        //     // Relacionamento com User
        //     entity
        //         .HasOne(uc => uc.User)
        //         .WithMany(u => u.UserClaims)
        //         .HasForeignKey(uc => uc.UserId)
        //         .OnDelete(DeleteBehavior.Cascade);
        // });
    }
}
