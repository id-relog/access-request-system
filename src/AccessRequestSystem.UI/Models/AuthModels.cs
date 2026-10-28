using System.ComponentModel.DataAnnotations;

namespace AccessRequestSystem.UI.Models;

public class LoginViewModel
{
    [Required, EmailAddress, Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required, EmailAddress, Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6), Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Повтор пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>То, что возвращает POST /api/auth/login и /api/auth/register.</summary>
public record AuthResponse(Guid UserId, string Token, DateTime ExpiresAt, List<string> Roles, string? Email = null);
