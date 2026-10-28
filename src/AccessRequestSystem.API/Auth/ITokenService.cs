using Microsoft.AspNetCore.Identity;

namespace AccessRequestSystem.API.Auth;

public interface ITokenService
{
    /// <summary>Генерирует подписанный JWT для пользователя с его ролями. Используется для вызовов API.</summary>
    string GenerateToken(IdentityUser<Guid> user, IList<string> roles);
}
