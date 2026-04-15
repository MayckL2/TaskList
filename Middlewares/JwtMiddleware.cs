using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace TaskList.Middlewares;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<JwtMiddleware> logger
    )
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var user = AttachUserFromToken(token);
            if (user != null)
            {
                context.User = user;
                _logger.LogDebug($"✅ Usuário autenticado: {user.Identity?.Name}");
            }
            else
            {
                _logger.LogWarning("❌ Falha ao autenticar token");
            }
        }
        else
        {
            _logger.LogDebug("🔑 Nenhum token encontrado na requisição");
        }

        await _next(context);
    }

    private ClaimsPrincipal? AttachUserFromToken(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
            {
                _logger.LogError("❌ JWT SecretKey inválida ou muito curta (mínimo 32 caracteres)");
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            // 🔥 VALIDAÇÃO COMPLETA DO TOKEN
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role, // 👈 Mapeia roles corretamente
            };

            var principal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out SecurityToken validatedToken
            );

            var jwtToken = validatedToken as JwtSecurityToken;
            if (jwtToken == null)
            {
                _logger.LogWarning("❌ Token não é um JWT válido");
                return null;
            }

            // 🔥 LOG DETALHADO DAS CLAIMS (para debug)
            _logger.LogDebug($"📋 Claims do token:");
            foreach (var claim in principal.Claims)
            {
                _logger.LogDebug($"   {claim.Type}: {claim.Value}");
            }

            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning($"⏰ Token expirado: {ex.Message}");
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogWarning($"🔑 Assinatura do token inválida: {ex.Message}");
        }
        catch (SecurityTokenInvalidIssuerException ex)
        {
            _logger.LogWarning($"🏢 Issuer inválido: {ex.Message}");
        }
        catch (SecurityTokenInvalidAudienceException ex)
        {
            _logger.LogWarning($"👥 Audience inválida: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"❌ Erro ao validar token: {ex.Message}");
        }

        return null;
    }
}
