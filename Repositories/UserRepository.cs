using System.Security.Claims;
using AutoMapper;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskList.Contexts;
using TaskList.DTOs;
using TaskList.Models;

namespace TaskList.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TaskContext _context;
    private readonly ILogger<UserRepository> _logger;
    private readonly IMapper _mapper;

    public UserRepository(TaskContext context, ILogger<UserRepository> logger, IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id.Equals(id));
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context
            .Users.Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u =>
                u.RefreshTokens.Any(t => t.Token == refreshToken && t.IsActive)
            );
    }

    public async Task<bool> CreateAsync(AuthUser user, string password)
    {
        var createUser = _mapper.Map<User>(user);
        createUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        createUser.CreatedAt = DateTime.UtcNow;
        createUser.IsActive = true;
        createUser.Id = "0";
        _context.Users.Add(createUser);

        try
        {
            int affectedRows = await _context.SaveChangesAsync();
            if (affectedRows > 0)
            {
                Console.WriteLine($"User created sucefully!: {createUser.Id}");
            }
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"❌ Database error: {ex.InnerException?.Message ?? ex.Message}");
            throw;
        }

        return true;
    }

    public async Task<bool> UpdateAsync(AuthUser user)
    {
        var UpdateUser = _mapper.Map<User>(user);
        UpdateUser.UpdatedAt = DateTime.UtcNow;
        _context.SaveChanges();
        var updatedUser = await this.GetByIdAsync(user.Id.ToString());
        if (updatedUser == null)
        {
            throw new KeyNotFoundException(
                $"User ID {user.Id.ToString()} not found after updated."
            );
        }
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await GetByIdAsync(id);
        if (user == null)
            return false;

        // Soft delete
        user.IsActive = false;
        user.DeletedAt = DateTime.UtcNow;

        _context.SaveChanges();
        var deletedUser = await this.GetByIdAsync(user.Id.ToString());
        if (deletedUser == null)
        {
            throw new KeyNotFoundException(
                $"User ID {user.Id.ToString()} not found after updated."
            );
        }
        return true;
    }

    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        if (user == null)
            return false;

        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        // Remover tokens antigos do mesmo usuário
        var oldTokens = _context.RefreshTokens.Where(t => t.UserId == user.Id && !t.IsUsed);
        _context.RefreshTokens.RemoveRange(oldTokens);

        // Gerar novo token
        var token = new RefreshToken
        {
            UserId = user.Id,
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            Expires = DateTime.UtcNow.AddHours(1), // Token válido por 1 hora
            CreatedAt = DateTime.UtcNow,
            IsUsed = false,
        };

        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        return token.Token;
    }

    public async Task<bool> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        // Buscar token válido
        var resetToken = await _context.RefreshTokens.FirstOrDefaultAsync(t =>
            t.UserId == userId && t.Token == token && !t.IsUsed && t.Expires > DateTime.UtcNow
        );

        if (resetToken == null)
            return false;

        // Buscar usuário
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        // 🔥 Atualizar senha com BCrypt
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        // Marcar token como usado
        resetToken.IsUsed = true;

        // Revogar todos os refresh tokens do usuário (segurança)
        if (user.RefreshTokens != null)
        {
            foreach (var rt in user.RefreshTokens)
            {
                rt.RevokedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task AddRefreshTokenAsync(User user, RefreshToken refreshToken)
    {
        user.RefreshTokens ??= new List<RefreshToken>();
        user.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt =>
            rt.Token == refreshToken
        );

        if (token != null)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
