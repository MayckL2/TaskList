using System.ComponentModel.DataAnnotations;

namespace TaskList.DTOs;

public class RefreshTokenDto
{
    [Required(ErrorMessage = "Refresh token é obrigatório")]
    public string RefreshToken { get; set; } = string.Empty;
}
