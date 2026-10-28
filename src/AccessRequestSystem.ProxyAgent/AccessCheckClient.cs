using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace AccessRequestSystem.ProxyAgent;

public class AccessCheckClient
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AccessCheckClient> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(20);

    public AccessCheckClient(HttpClient http, IMemoryCache cache, ILogger<AccessCheckClient> logger)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Проверяет доступ. Принимает токен из Cookie браузера.
    /// </summary>
    public async Task<bool> IsAllowedAsync(string domain, string jwtToken, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(jwtToken))
        {
            _logger.LogWarning("Пустой токен для {Domain}", domain);
            return false;
        }

        var cacheKey = $"{domain}_{jwtToken.GetHashCode()}";

        if (_cache.TryGetValue(cacheKey, out bool cached))
            return cached;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/access/check?domain={Uri.EscapeDataString(domain)}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _http.SendAsync(request, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Токен недействителен или истек для {Domain}", domain);
                return false;
            }

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<CheckResponse>(cancellationToken: ct);
            var allowed = body?.HasAccess ?? false;

            _cache.Set(cacheKey, allowed, CacheDuration);
            return allowed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка проверки доступа к {Domain}", domain);
            return false;
        }
    }

    private record CheckResponse(string Domain, bool HasAccess);
}