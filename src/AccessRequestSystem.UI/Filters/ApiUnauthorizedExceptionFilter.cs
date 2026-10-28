using AccessRequestSystem.UI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AccessRequestSystem.UI.Filters;

/// <summary>
/// : IAsyncExceptionFilter, Cookie живёт дольше, чем JWT Когда токен истекает,
/// API начинает отвечать 401 — этот фильтр ловит такую ситуацию в любом контроллере,
/// разлогинивает пользователя и отправляет его на страницу входа вместо страницы с ошибкой.
/// </summary>
public class ApiUnauthorizedExceptionFilter : IAsyncExceptionFilter
{
    public async Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not ApiUnauthorizedException)
            return;

        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
        context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
        context.ExceptionHandled = true;
    }
}
