using System.Net.Http.Json;
using System.Text.Json;
using AccessRequestSystem.UI.Models;

namespace AccessRequestSystem.UI.Services;

public class ApiClient : IApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
        => await SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/login", new { email, password });

    public async Task<AuthResponse> RegisterAsync(string email, string password)
        => await SendAsync<AuthResponse>(HttpMethod.Post, "api/auth/register", new { email, password });

    public async Task<EmployeeDto?> GetMyProfileAsync()
    {
        var response = await _http.GetAsync("api/employees/me");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<EmployeeDto>(JsonOptions);
    }

    public async Task<EmployeeDto> UpsertProfileAsync(EmployeeFormViewModel dto)
        => await SendAsync<EmployeeDto>(HttpMethod.Put, "api/employees/me", dto);

    public async Task<List<ServiceDto>> GetActiveServicesAsync()
        => await SendAsync<List<ServiceDto>>(HttpMethod.Get, "api/services");

    public async Task<List<ServiceDto>> GetAllServicesAsync()
        => await SendAsync<List<ServiceDto>>(HttpMethod.Get, "api/services/all");

    public async Task<ServiceDto> CreateServiceAsync(ServiceFormViewModel dto)
        => await SendAsync<ServiceDto>(HttpMethod.Post, "api/services", dto);

    public async Task<ServiceDto> UpdateServiceAsync(Guid id, ServiceFormViewModel dto)
        => await SendAsync<ServiceDto>(HttpMethod.Put, $"api/services/{id}", dto);

    public async Task<AccessRequestDto> CreateRequestAsync(CreateAccessRequestBody body)
        => await SendAsync<AccessRequestDto>(HttpMethod.Post, "api/requests", body);

    public async Task<List<AccessRequestDto>> GetMyRequestsAsync()
        => await SendAsync<List<AccessRequestDto>>(HttpMethod.Get, "api/requests/my");

    public async Task<List<AccessRequestDto>> GetAllRequestsAsync(RequestStatus? status)
    {
        var query = status is null ? string.Empty : $"?status={(int)status.Value}";
        return await SendAsync<List<AccessRequestDto>>(HttpMethod.Get, $"api/requests{query}");
    }

    public async Task<AccessRequestDto> DecideAsync(Guid id, DecisionBody body)
        => await SendAsync<AccessRequestDto>(HttpMethod.Put, $"api/requests/{id}/decision", body);

    public async Task<List<AccessLogDto>> GetAccessLogsAsync(int count = 200, string? search = null)
    {
        var url = $"api/access-logs?count={count}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&q={Uri.EscapeDataString(search.Trim())}";
        return await SendAsync<List<AccessLogDto>>(HttpMethod.Get, url);
    }

    public async Task<AuthResponse?> ExchangeSsoTicketAsync(string ticket)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/sso-tickets/exchange")
        {
            Content = JsonContent.Create(new { ticket }, options: JsonOptions)
        };

        var response = await _http.SendAsync(request);
        if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.BadRequest)
            return null;

        await EnsureSuccess(response);
        return await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string url, object? body = null)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        var response = await _http.SendAsync(request);
        await EnsureSuccess(response);

        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return result!;
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var raw = await response.Content.ReadAsStringAsync();
        var message = ExtractErrorMessage(raw) ?? $"Ошибка запроса к API: {(int)response.StatusCode} {response.ReasonPhrase}";

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new ApiUnauthorizedException(message);

        throw new ApiException(message);
    }

    /// <summary>API возвращает ошибки в разных формах массив строк Identity, либо просто текст.</summary>
    private static string? ExtractErrorMessage(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.ValueKind == JsonValueKind.Object && doc.RootElement.TryGetProperty("error", out var errorProp))
                return errorProp.GetString();

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
                return string.Join("; ", doc.RootElement.EnumerateArray().Select(e => e.ToString()));
        }
        catch (JsonException)
        {
            return raw.Trim('"');
        }

        return raw;
    }
}
