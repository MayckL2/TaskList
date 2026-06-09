using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskList.Models;
using TaskList.Repositories;

namespace TaskList.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user, IList<string> roles);
    Task<string> GenerateRefreshTokenAsync(User user, string ipAddress);
    Task<RefreshToken?> ValidateRefreshTokenAsync(string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        IUserRepository userRepository,
        ILogger<TokenService> logger
    )
    {
        _jwtSettings = jwtSettings.Value;
        _userRepository = userRepository;
        _logger = logger;
    }

    public string GenerateAccessToken(User user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
            new Claim("full_name", user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString()
            ),
        };

        // Add roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(User user, string ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
        };

        await _userRepository.AddRefreshTokenAsync(refreshToken);

        return refreshToken.Token;
    }

    public async Task<RefreshToken?> ValidateRefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);

        if (user == null)
            return null;

        var token = user.RefreshTokens?.FirstOrDefault(rt =>
            rt.Token == refreshToken && rt.IsActive
        );

        // Rotation detection: if token was revoked, revoke all user tokens (possible theft)
        if (token != null && token.RevokedAt != null)
        {
            await _userRepository.RevokeAllUserRefreshTokensAsync(user.Id);
            _logger.LogWarning(
                "Refresh token reuse detected for user {UserId}. All tokens revoked.",
                user.Id
            );
            return null;
        }

        return token;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        await _userRepository.RevokeRefreshTokenAsync(refreshToken);
    }
}
