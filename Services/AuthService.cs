using AutoMapper;
using Microsoft.AspNetCore.Identity;
using TaskList.DTOs;
using TaskList.Models;
using TaskList.Repositories;

namespace TaskList.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IMapper mapper,
        ILogger<AuthService> logger
    )
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDTO registerDto)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            return new AuthResponseDto { Success = false, Message = "Email already registered" };
        }

        // Create new user
        var user = _mapper.Map<User>(registerDto);
        var created = await _userRepository.CreateAsync(user, registerDto.Password);

        if (!created)
        {
            return new AuthResponseDto { Success = false, Message = "Failed to create user" };
        }

        // Get created user
        user = await _userRepository.GetByEmailAsync(registerDto.Email);
        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "User not found after creation",
            };
        }

        // Generate email confirmation token (for real implementation)
        var confirmationToken = await _userRepository.GenerateEmailConfirmationTokenAsync(user);

        // In production: send email with confirmation link
        _logger.LogInformation(
            "User registered: {Email}. Confirmation token: {Token}",
            user.Email,
            confirmationToken
        );

        return new AuthResponseDto
        {
            Success = true,
            Message = "User registered successfully. Please confirm your email.",
            User = _mapper.Map<UserResponseDto>(user),
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, string ipAddress)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(loginDto.Email);

        if (user == null || !user.IsActive)
        {
            return new AuthResponseDto { Success = false, Message = "Invalid email or password" };
        }

        // Check if user is locked out
        if (await _userRepository.IsLockedOutAsync(user))
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Account is locked out. Please try again later.",
            };
        }

        // Verify password
        var isValid = await _userRepository.CheckPasswordAsync(user, loginDto.Password);

        if (!isValid)
        {
            var failedCount = await _userRepository.IncrementAccessFailedCountAsync(user);
            _logger.LogWarning(
                "Failed login attempt for {Email}. Attempt {FailedCount}",
                loginDto.Email,
                failedCount
            );

            return new AuthResponseDto { Success = false, Message = "Invalid email or password" };
        }

        // Reset failed attempts on successful login
        await _userRepository.ResetAccessFailedCountAsync(user);

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Generate tokens
        var roles = await _userRepository.GetUserRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, ipAddress);

        var userResponse = _mapper.Map<UserResponseDto>(user);
        userResponse.Roles = roles;

        return new AuthResponseDto
        {
            Success = true,
            Message = "Login successful",
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 900, // 15 minutes in seconds
            User = userResponse,
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        RefreshTokenDto refreshTokenDto,
        string ipAddress
    )
    {
        var refreshToken = await _tokenService.ValidateRefreshTokenAsync(
            refreshTokenDto.RefreshToken
        );

        if (refreshToken == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid or expired refresh token",
            };
        }

        var user = refreshToken.User;

        if (user == null || !user.IsActive)
        {
            return new AuthResponseDto { Success = false, Message = "User not found or inactive" };
        }

        // Revoke old token
        await _tokenService.RevokeRefreshTokenAsync(refreshToken.Token);

        // Generate new tokens
        var roles = await _userRepository.GetUserRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user, ipAddress);

        // Update user last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        var userResponse = _mapper.Map<UserResponseDto>(user);
        userResponse.Roles = roles;

        return new AuthResponseDto
        {
            Success = true,
            Message = "Token refreshed successfully",
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 900,
            User = userResponse,
        };
    }

    public async Task<AuthResponseDto> LogoutAsync(string refreshToken)
    {
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);

        return new AuthResponseDto { Success = true, Message = "Logout successful" };
    }

    public async Task<AuthResponseDto> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return new AuthResponseDto { Success = false, Message = "User not found" };
        }

        var result = await _userRepository.ConfirmEmailAsync(user, token);

        if (!result)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid or expired confirmation token",
            };
        }

        return new AuthResponseDto { Success = true, Message = "Email confirmed successfully" };
    }

    public async Task<AuthResponseDto> ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        // Don't reveal if user exists (security)
        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = true,
                Message = "If your email is registered, you will receive a password reset link",
            };
        }

        var token = await _userRepository.GeneratePasswordResetTokenAsync(user);

        // In production: send email with reset link
        _logger.LogInformation(
            "Password reset requested for {Email}. Token: {Token}",
            email,
            token
        );

        return new AuthResponseDto
        {
            Success = true,
            Message = "If your email is registered, you will receive a password reset link",
        };
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(
        string email,
        string token,
        string newPassword
    )
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null)
        {
            return new AuthResponseDto { Success = false, Message = "Invalid request" };
        }

        var result = await _userRepository.ResetPasswordAsync(user, token, newPassword);

        if (!result)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "Invalid or expired reset token",
            };
        }

        return new AuthResponseDto { Success = true, Message = "Password reset successfully" };
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return null;

        var roles = await _userRepository.GetUserRolesAsync(user);
        var userResponse = _mapper.Map<UserResponseDto>(user);
        userResponse.Roles = roles;

        return userResponse;
    }
}
