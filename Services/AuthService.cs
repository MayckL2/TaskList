using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskList.DTOs;
using TaskList.Models;
using TaskList.Repositories;
using static TaskList.DTOs.RegisterDTO;

namespace TaskList.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtSettings _jwtSettings;
    private readonly IMapper _mapper;
    private readonly IRoleRepository _roles;

    public AuthService(
        IUserRepository userRepository,
        IEmailService emailService,
        IConfiguration configuration,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger,
        IMapper mapper,
        IRoleRepository roles
    )
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _configuration = configuration;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
        _mapper = mapper;
        _roles = roles;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDTO registerDto)
    {
        try
        {
            // Verificar se usuário já existe
            var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto { Success = false, Message = "Email já está em uso" };
            }

            // Criar novo usuário
            var user = new AuthUser()
            {
                FullName = registerDto.Email,
                Email = registerDto.Email,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };

            var created = await _userRepository.CreateAsync(user, registerDto.Password);
            if (!created)
            {
                return new AuthResponseDto { Success = false, Message = "Erro ao criar usuário" };
            }

            // Buscar usuário criado
            var UserEmail = _mapper.Map<User>(user);
            UserEmail = await _userRepository.GetByEmailAsync(registerDto.Email);

            // Gerar tokens
            var UserToken = _mapper.Map<User>(user);
            var tokens = await GenerateTokensAsync(UserToken);

            // Enviar email de boas-vindas
            await _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

            _logger.LogInformation("Novo usuário registrado: {Email}", user.Email);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Usuário registrado com sucesso",
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = MapToUserResponse(user),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar usuário: {Email}", registerDto.Email);
            return new AuthResponseDto
            {
                Success = false,
                Message = "Erro interno ao processar registro",
            };
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        try
        {
            // Buscar usuário
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null || !user.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email ou senha inválidos",
                };
            }

            // Verificar senha
            var validPassword = await _userRepository.CheckPasswordAsync(user, loginDto.Password);
            if (!validPassword)
            {
                _logger.LogWarning("Tentativa de login inválida para: {Email}", loginDto.Email);
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email ou senha inválidos",
                };
            }

            // Atualizar último login
            user.LastLoginAt = DateTime.UtcNow;
            var UserLastLogin = _mapper.Map<AuthUser>(user);
            await _userRepository.UpdateAsync(UserLastLogin);

            // Gerar tokens
            var UserToken = _mapper.Map<User>(user);
            var tokens = await GenerateTokensAsync(UserToken);

            _logger.LogInformation("Login bem-sucedido: {Email}", user.Email);

            var UserResponse = _mapper.Map<AuthUser>(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login realizado com sucesso",
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = MapToUserResponse(UserResponse),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar login: {Email}", loginDto.Email);
            return new AuthResponseDto
            {
                Success = false,
                Message = "Erro interno ao processar login",
            };
        }
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Buscar usuário pelo refresh token
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
            if (user == null)
            {
                return new AuthResponseDto { Success = false, Message = "Refresh token inválido" };
            }

            // Verificar se o token específico ainda é válido
            var token = user.RefreshTokens?.FirstOrDefault(rt => rt.Token == refreshToken);
            if (token == null || !token.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Refresh token expirado ou revogado",
                };
            }

            // Revogar o token atual e gerar novo
            token.RevokedAt = DateTime.UtcNow;
            var tokens = await GenerateTokensAsync(user);

            _logger.LogInformation("Token renovado para: {Email}", user.Email);

            var UserResponse = _mapper.Map<AuthUser>(user);

            return new AuthResponseDto
            {
                Success = true,
                AccessToken = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                User = MapToUserResponse(UserResponse),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renovar token");
            return new AuthResponseDto
            {
                Success = false,
                Message = "Erro interno ao renovar token",
            };
        }
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(forgotPasswordDto.Email);
            if (user == null || !user.IsActive)
            {
                // Por segurança, não revelar se usuário existe ou não
                _logger.LogInformation(
                    "Tentativa de reset de senha para email não cadastrado: {Email}",
                    forgotPasswordDto.Email
                );
                return true;
            }

            // Gerar token de reset
            var resetToken = await _userRepository.GeneratePasswordResetTokenAsync(user);

            // Criar link de reset (ajuste a URL conforme seu frontend)
            var resetLink =
                $"https://seusite.com/reset-password?userId={user.Id}&token={Uri.EscapeDataString(resetToken)}";

            // Enviar email
            await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetLink);

            _logger.LogInformation("Email de reset de senha enviado para: {Email}", user.Email);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao processar forgot password: {Email}",
                forgotPasswordDto.Email
            );
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(resetPasswordDto.UserId);
            if (user == null || !user.IsActive)
                return false;

            var resetResult = await _userRepository.ResetPasswordAsync(
                user.Id,
                resetPasswordDto.Token,
                resetPasswordDto.NewPassword
            );

            if (resetResult)
            {
                // Enviar email de confirmação
                await _emailService.SendPasswordChangedEmailAsync(user.Email, user.FullName);
                _logger.LogInformation("Senha alterada com sucesso para: {Email}", user.Email);
            }

            return resetResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao resetar senha para usuário: {UserId}",
                resetPasswordDto.UserId
            );
            return false;
        }
    }

    public async Task<bool> LogoutAsync(string userId, string refreshToken)
    {
        try
        {
            await _userRepository.RevokeRefreshTokenAsync(refreshToken);
            _logger.LogInformation("Logout realizado: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer logout: {UserId}", userId);
            return false;
        }
    }

    public async Task<UserResponseDto> GetUserProfileAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return null;

        var UserResponse = _mapper.Map<AuthUser>(user);

        return MapToUserResponse(UserResponse);
    }

    // Métodos privados auxiliares
    private async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = await GenerateRefreshTokenAsync(user);

        return (accessToken.ToString(), refreshToken);
    }

    private async Task<string> GenerateAccessToken(User user)
    {
        var roles = await _roles.GetUserRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("user_id", user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString()
            ),
        };

        // Adicionar roles como claims
        foreach (var role in roles)
        {
            claims.Add(
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role)
            );
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<string> GenerateRefreshTokenAsync(User user)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = "0.0.0.0", // Em produção, pegar IP real
        };

        await _userRepository.AddRefreshTokenAsync(user, refreshToken);
        return refreshToken.Token;
    }

    private UserResponseDto MapToUserResponse(AuthUser user)
    {
        var response = _mapper.Map<User>(user);

        return new UserResponseDto
        {
            Id = response.Id,
            FullName = response.FullName,
            Email = response.Email,
            CreatedAt = response.CreatedAt,
            LastLoginAt = response.LastLoginAt,
            Roles = _roles.GetUserRolesAsync(response).Result,
        };
    }
}

// Configurações JWT
public class JwtSettings
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}
