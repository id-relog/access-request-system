using AccessRequestSystem.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestSystem.UI.Controllers;

[AllowAnonymous]
public class ProxyController : Controller
{
    private readonly IApiClient _api;

    public ProxyController(IApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Blocked(string? domain, string? ticket)
    {
        if (!string.IsNullOrWhiteSpace(ticket) && User.Identity?.IsAuthenticated != true)
        {
            var auth = await _api.ExchangeSsoTicketAsync(ticket);
            if (auth is not null)
            {
                var email = !string.IsNullOrWhiteSpace(auth.Email) ? auth.Email! : "user";
                await CookieSignIn.SignInAsync(HttpContext, email, auth);
                // cookie уже в ответе; чистый URL без ticket
                return RedirectToAction(nameof(Blocked), new { domain });
            }
        }

        ViewBag.Domain = domain ?? "неизвестный сайт";
        return View();
    }
}
