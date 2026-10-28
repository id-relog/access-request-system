using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;

namespace AccessRequestSystem.ProxyAgent;

public class ProxyHostedService : IHostedService
{
    private readonly ProxyServer _proxyServer = new();
    private readonly AccessCheckClient _accessCheck;
    private readonly ApiAuthClient _apiAuth;
    private readonly SsoTicketClient _ssoTickets;
    private readonly ProxyAuthState _authState;
    private readonly ILogger<ProxyHostedService> _logger;
    private readonly int _port;
    private readonly string _uiBaseUrl;
    private readonly int _tokenCacheMinutes = 5;
    private ExplicitProxyEndPoint? _endPoint;

    public ProxyHostedService(
        AccessCheckClient accessCheck,
        ApiAuthClient apiAuth,
        SsoTicketClient ssoTickets,
        ProxyAuthState authState,
        IConfiguration config,
        ILogger<ProxyHostedService> logger)
    {
        _accessCheck = accessCheck;
        _apiAuth = apiAuth;
        _ssoTickets = ssoTickets;
        _authState = authState;
        _logger = logger;
        _port = int.TryParse(config["Proxy:Port"], out var p) ? p : 8000;
        _uiBaseUrl = (config["Proxy:UiBaseUrl"] ?? "https://localhost:7050").TrimEnd('/');
        if (int.TryParse(config["Proxy:TokenCacheMinutes"], out var minutes))
            _tokenCacheMinutes = Math.Max(1, minutes);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _authState.SetRealmChangedHandler(realm => _proxyServer.ProxyAuthenticationRealm = realm);
        _proxyServer.ProxyAuthenticationRealm = _authState.Realm;
        _proxyServer.ProxyBasicAuthenticateFunc = AuthenticateAsync;

        _proxyServer.CertificateManager.EnsureRootCertificate();

        _endPoint = new ExplicitProxyEndPoint(System.Net.IPAddress.Any, _port, decryptSsl: true);
        _endPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest;
        _proxyServer.BeforeRequest += OnBeforeRequest;

        _proxyServer.AddEndPoint(_endPoint);
        _proxyServer.Start();

        var cert = _proxyServer.CertificateManager.RootCertificate;
        var certBytes = cert.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Cert);
        var certPath = Path.Combine(AppContext.BaseDirectory, "proxy-root-cert.cer");
        File.WriteAllBytes(certPath, certBytes);
        _logger.LogInformation("Корневой сертификат экспортирован в {CertPath}", certPath);

        _logger.LogInformation("Прокси-агент запущен на порту {Port}.", _port);
        _logger.LogInformation("UI: {UiUrl}", _uiBaseUrl);
        _logger.LogInformation("Запуск Chrome: chrome.exe --proxy-server=127.0.0.1:{Port}", _port);
        _logger.LogInformation("{Result,-6}| {Account,-25}| {Url}", "RESULT", "ACCOUNT", "URL");

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_endPoint is not null)
            _endPoint.BeforeTunnelConnectRequest -= OnBeforeTunnelConnectRequest;

        _proxyServer.BeforeRequest -= OnBeforeRequest;
        _proxyServer.ProxyBasicAuthenticateFunc = null;
        _proxyServer.Stop();
        _logger.LogInformation("Прокси-агент остановлен");
        return Task.CompletedTask;
    }

    private async Task<bool> AuthenticateAsync(SessionEventArgsBase session, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || password is null)
            return false;

        var passwordHash = ComputeHash(password);

        if (_authState.TryGetSession(username, out var cached) &&
            cached.ExpiresAt > DateTime.UtcNow &&
            cached.PasswordHash == passwordHash)
        {
            session.UserData = username;
            return true;
        }

        _logger.LogInformation("Логин в API для {Username}", username);
        var result = await _apiAuth.LoginAsync(username, password);

        if (!result.Success || string.IsNullOrEmpty(result.Token))
        {
            _logger.LogWarning("Логин не удался для {Username}", username);
            return false;
        }

        var effectiveExpiry = result.ExpiresAt > DateTime.UtcNow.AddMinutes(_tokenCacheMinutes)
            ? DateTime.UtcNow.AddMinutes(_tokenCacheMinutes)
            : result.ExpiresAt;

        // Новый логин — снова нужен SSO в UI
        _authState.SetSession(username, new ProxyUserSession(result.Token, effectiveExpiry, passwordHash, SsoDone: false));
        session.UserData = username;
        _logger.LogInformation("Токен получен для {Username}, кэш до {Effective}", username, effectiveExpiry);
        return true;
    }

    private Task OnBeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
    {
        var host = e.HttpClient.Request.RequestUri.Host;
        if (IsLocal(host))
            return Task.CompletedTask;

        if (e.UserData is string username)
            e.WebSession.UserData = username;

        return Task.CompletedTask;
    }

    private async Task OnBeforeRequest(object sender, SessionEventArgs e)
    {
        var requestUri = e.HttpClient.Request.RequestUri;
        var host = requestUri.Host;

        if (IsLocal(host))
            return;

        var username = GetProxyUsername(e.HttpClient.Request)
            ?? e.UserData as string
            ?? e.WebSession.UserData as string;

        if (string.IsNullOrEmpty(username) ||
            !_authState.TryGetSession(username, out var session) ||
            session.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Нет действующей сессии для {Url}", e.HttpClient.Request.Url);
            return;
        }

        // После первого входа в прокси — один раз залогинить в UI через SSO, потом вернуть на сайт
        if (!session.SsoDone)
        {
            var ticket = await _ssoTickets.CreateTicketAsync(session.Token);
            if (!string.IsNullOrEmpty(ticket))
            {
                _authState.MarkSsoDone(username);
                var returnUrl = e.HttpClient.Request.Url;
                var ssoUrl =
                    $"{_uiBaseUrl}/Account/Sso?ticket={Uri.EscapeDataString(ticket)}&returnUrl={Uri.EscapeDataString(returnUrl)}";
                _logger.LogInformation("SSO → UI для {Username}, затем обратно на {Url}", username, returnUrl);
                e.Redirect(ssoUrl);
                return;
            }

            _logger.LogWarning("SSO-билет не создан для {Username}, продолжаем без авто-входа в UI", username);
            _authState.MarkSsoDone(username);
        }

        var allowed = await _accessCheck.IsAllowedAsync(host, session.Token);
        LogAccess(allowed, username, e.HttpClient.Request.Url);
        if (!allowed)
        {
            var ticket = await _ssoTickets.CreateTicketAsync(session.Token);
            var blockedUrl = $"{_uiBaseUrl}/Proxy/Blocked?domain={Uri.EscapeDataString(host)}";
            if (!string.IsNullOrEmpty(ticket))
                blockedUrl += $"&ticket={Uri.EscapeDataString(ticket)}";

            _logger.LogWarning("BLOCKED: {Url}", e.HttpClient.Request.Url);
            e.Redirect(blockedUrl);
        }
    }

    private static bool IsLocal(string host) =>
        host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
        host.Equals("127.0.0.1", StringComparison.Ordinal) ||
        host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase);

    private static string? GetProxyUsername(Request request)
    {
        var header = request.Headers.FirstOrDefault(h =>
            h.Name.Equals("Proxy-Authorization", StringComparison.OrdinalIgnoreCase));
        if (header is null) return null;

        const string prefix = "Basic ";
        if (!header.Value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header.Value[prefix.Length..]));
            var separatorIndex = decoded.IndexOf(':');
            return separatorIndex < 0 ? decoded : decoded[..separatorIndex];
        }
        catch
        {
            return null;
        }
    }

    private static string ComputeHash(string input)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }

    private void LogAccess(bool allowed, string account, string url)
    {
        const string template = "{Result,-6}| {Account,-25}| {Url}";
        if (allowed)
            _logger.LogInformation(template, "ALLOW", account, url);
        else
            _logger.LogWarning(template, "BLOCK", account, url);
    }
}
