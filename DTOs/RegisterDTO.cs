using System.ComponentModel.DataAnnotations;

namespace TaskList.DTOs;

public class RegisterDTO
{
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage = "O nome deve ter entre 3 e 100 caracteres"
    )]
    public required string FullName { get; set; }

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(100, ErrorMessage = "O email deve ter no máximo 100 caracteres")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "A senha é obrigatória")]
    [StringLength(
        100,
        MinimumLength = 6,
        ErrorMessage = "A senha deve ter entre 6 e 100 caracteres"
    )]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
        ErrorMessage = "A senha deve conter letra maiúscula, minúscula, número e caractere especial"
    )]
    public required string Password { get; set; }

    [Compare("Password", ErrorMessage = "As senhas não coincidem")]
    public required string ConfirmPassword { get; set; }

    public class LoginDto
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        public required string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public required string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required]
        public required string UserId { get; set; }

        [Required]
        public required string Token { get; set; }

        [Required(ErrorMessage = "A nova senha é obrigatória")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "A senha deve ter entre 6 e 100 caracteres"
        )]
        public required string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "As senhas não coincidem")]
        public required string ConfirmPassword { get; set; }
    }

    public class UserResponseDto
    {
        public required string Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public required List<string> Roles { get; set; }
    }

    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserResponseDto User { get; set; }
    }
}
