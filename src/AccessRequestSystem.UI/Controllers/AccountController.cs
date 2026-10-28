using AccessRequestSystem.UI.Models;
using AccessRequestSystem.UI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestSystem.UI.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IApiClient _api;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IApiClient api,
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<AccountController> logger)
    {
        _api = api;
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Welcome() => View();

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
        => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var auth = await _api.LoginAsync(model.Email, model.Password);
            await CookieSignIn.SignInAsync(HttpContext, model.Email, auth);
            return RedirectToLocal(model.ReturnUrl);
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    /// <summary>Одноразовый вход из прокси-агента (без повторного ввода пароля).</summary>
    [HttpGet]
    public async Task<IActionResult> Sso(string? ticket, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(ticket))
        {
            TempData["Error"] = "Нет SSO-билета. Откройте сайт через прокси или войдите вручную.";
            return RedirectToAction(nameof(Welcome));
        }

        var auth = await _api.ExchangeSsoTicketAsync(ticket);
        if (auth is null)
        {
            TempData["Error"] = "SSO-билет недействителен или уже использован. Войдите снова.";
            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        var email = !string.IsNullOrWhiteSpace(auth.Email) ? auth.Email! : "user";
        await CookieSignIn.SignInAsync(HttpContext, email, auth);
        return RedirectAfterSso(returnUrl);
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var auth = await _api.RegisterAsync(model.Email, model.Password);
            await CookieSignIn.SignInAsync(HttpContext, model.Email, auth);
            return RedirectToAction("Edit", "Profile");
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await InvalidateProxyAsync();
        TempData["Success"] = "Вы вышли. При следующем сайте браузер снова запросит логин и пароль прокси.";
        return RedirectToAction(nameof(Welcome));
    }

    public IActionResult AccessDenied() => View();

    private async Task InvalidateProxyAsync()
    {
        try
        {
            var baseUrl = _config["ProxyAgent:ControlUrl"] ?? "http://127.0.0.1:8001";
            var key = _config["ProxyAgent:ControlKey"] ?? "dev-proxy-key";
            var client = _httpClientFactory.CreateClient("ProxyControl");
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/invalidate");
            request.Headers.TryAddWithoutValidation("X-Proxy-Key", key);
            var response = await client.SendAsync(request);
            _logger.LogInformation("Invalidate прокси: {Status}", response.StatusCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось сбросить прокси (агент не запущен?)");
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl)
        => !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("My", "Requests");

    private IActionResult RedirectAfterSso(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) &&
            Uri.TryCreate(returnUrl, UriKind.Absolute, out var absolute) &&
            (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
            return Redirect(returnUrl);

        return RedirectToLocal(returnUrl);
    }
}
