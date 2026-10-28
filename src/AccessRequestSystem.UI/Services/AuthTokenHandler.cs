using System.Net.Http.Headers;

namespace AccessRequestSystem.UI.Services;

/// <summary>
/// Перед каждым запросом к API достаёт JWT-токен из claim текущего пользователя
/// и подставляет его в заголовок Authorization.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    public const string TokenClaimType = "access_token";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthTokenHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _httpContextAccessor.HttpContext?.User.FindFirst(TokenClaimType)?.Value;

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
