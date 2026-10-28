using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace AccessRequestSystem.ProxyAgent;

/// <summary>
/// Логинится в API по переданным логину и паролю (их присылает браузер через Proxy-Authorization,
///  ProxyHostedService.AuthenticateAsync) и возвращает JWT.
/// </summary>
public class ApiAuthClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ApiAuthClient> _logger;

    public ApiAuthClient(HttpClient http, ILogger<ApiAuthClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { email, password }, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Логин не удался для {Email}: {Status}", email, response.StatusCode);
                return LoginResult.Failed;
            }

            var body = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: ct);
            if (body is null || string.IsNullOrEmpty(body.Token))
                return LoginResult.Failed;

            return new LoginResult(true, body.Token, body.ExpiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при логине {Email} в API", email);
            return LoginResult.Failed;
        }
    }

    private record LoginResponse(Guid UserId, string Token, DateTime ExpiresAt, List<string> Roles);
}

public record LoginResult(bool Success, string? Token = null, DateTime ExpiresAt = default)
{
    public static readonly LoginResult Failed = new(false);
}
