using System.Security.Claims;
using AccessRequestSystem.UI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AccessRequestSystem.UI.Services;

public static class CookieSignIn
{
    public static async Task SignInAsync(HttpContext httpContext, string email, AuthResponse auth)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, auth.UserId.ToString()),
            new(ClaimTypes.Name, email),
            new(AuthTokenHandler.TokenClaimType, auth.Token)
        };
        claims.AddRange(auth.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}
