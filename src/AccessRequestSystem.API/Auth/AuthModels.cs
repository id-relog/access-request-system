using System.ComponentModel.DataAnnotations;

namespace AccessRequestSystem.API.Auth;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponse(Guid UserId, string Token, DateTime ExpiresAt, IReadOnlyList<string> Roles, string? Email = null);
