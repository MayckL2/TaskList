using System.ComponentModel.DataAnnotations;

namespace TaskList.DTOs;

public class LoginDto
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}
