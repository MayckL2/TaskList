using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskList.DTOs;
using TaskList.Services;
using static TaskList.DTOs.RegisterDTO;

namespace TaskList.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(registerDto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.LoginAsync(loginDto);

        if (!result.Success)
            return BadRequest(result);

        // Set refresh token in HTTP-only cookie
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken =
            Request.Cookies["refreshToken"]
            ?? (Request.Headers["X-Refresh-Token"].FirstOrDefault());

        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest(new { message = "Refresh token não fornecido" });

        var result = await _authService.RefreshTokenAsync(refreshToken);

        if (!result.Success)
            return BadRequest(result);

        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _authService.ForgotPasswordAsync(forgotPasswordDto);

        // Sempre retorna OK por segurança (não revelar se email existe)
        return Ok(new { message = "Se o email existir, você receberá instruções de recuperação" });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ResetPasswordAsync(resetPasswordDto);

        if (!result)
            return BadRequest(new { message = "Erro ao resetar senha" });

        return Ok(new { message = "Senha alterada com sucesso" });
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst("user_id")?.Value;
        var refreshToken = Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(refreshToken))
        {
            await _authService.LogoutAsync(userId, refreshToken);
        }

        // Remover cookie
        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logout realizado com sucesso" });
    }

    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst("user_id")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var profile = await _authService.GetUserProfileAsync(userId);

        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
