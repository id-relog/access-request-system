using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace AccessRequestSystem.ProxyAgent;

public class SsoTicketClient
{
    private readonly HttpClient _http;
    private readonly ILogger<SsoTicketClient> _logger;

    public SsoTicketClient(HttpClient http, ILogger<SsoTicketClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<string?> CreateTicketAsync(string jwtToken, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/sso-tickets");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Не удалось создать SSO-билет: {Status}", response.StatusCode);
                return null;
            }

            var body = await response.Content.ReadFromJsonAsync<CreateTicketResponse>(cancellationToken: ct);
            return body?.Ticket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка создания SSO-билета");
            return null;
        }
    }

    private record CreateTicketResponse(string Ticket, DateTime TicketExpiresAt);
}
