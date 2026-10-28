using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AccessRequestSystem.ProxyAgent;

/// <summary>
/// Локальный HTTP-контроль для UI: при Logout сбрасывает кэш прокси и меняет realm,
/// чтобы Chrome снова показал окно логина (смена сотрудника без закрытия браузера).
/// </summary>
public class ProxyControlHostedService : IHostedService, IDisposable
{
    private readonly ILogger<ProxyControlHostedService> _logger;
    private readonly IConfiguration _config;
    private readonly ProxyAuthState _authState;
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _loop;

    public ProxyControlHostedService(ProxyAuthState authState, IConfiguration config, ILogger<ProxyControlHostedService> logger)
    {
        _authState = authState;
        _config = config;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var port = int.TryParse(_config["Proxy:ControlPort"], out var p) ? p : 8001;
        var prefix = $"http://127.0.0.1:{port}/";

        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
        _listener.Start();

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loop = Task.Run(() => ListenLoopAsync(_cts.Token));

        _logger.LogInformation("Контроль прокси слушает {Prefix} (POST /invalidate)", prefix);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        _listener?.Stop();
        if (_loop is not null)
        {
            try { await _loop; }
            catch (OperationCanceledException) { }
        }
    }

    private async Task ListenLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _listener is { IsListening: true })
        {
            HttpListenerContext ctx;
            try
            {
                ctx = await _listener.GetContextAsync().WaitAsync(ct);
            }
            catch (Exception) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (HttpListenerException)
            {
                break;
            }

            _ = Task.Run(() => HandleAsync(ctx), CancellationToken.None);
        }
    }

    private async Task HandleAsync(HttpListenerContext ctx)
    {
        try
        {
            var req = ctx.Request;
            var res = ctx.Response;

            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = 204;
                res.Close();
                return;
            }

            var expectedKey = _config["Proxy:ControlKey"] ?? "dev-proxy-key";
            var key = req.Headers["X-Proxy-Key"];
            if (!string.Equals(key, expectedKey, StringComparison.Ordinal))
            {
                res.StatusCode = 401;
                await WriteAsync(res, "Unauthorized");
                return;
            }

            if (req.HttpMethod == "POST" &&
                string.Equals(req.Url?.AbsolutePath, "/invalidate", StringComparison.OrdinalIgnoreCase))
            {
                _authState.InvalidateAll("UI logout");
                res.StatusCode = 200;
                await WriteAsync(res, "{\"ok\":true}");
                return;
            }

            res.StatusCode = 404;
            await WriteAsync(res, "Not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка control-эндпоинта");
            try
            {
                ctx.Response.StatusCode = 500;
                ctx.Response.Close();
            }
            catch { /* ignore */ }
        }
    }

    private static async Task WriteAsync(HttpListenerResponse res, string body)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        res.ContentType = "application/json; charset=utf-8";
        res.ContentLength64 = bytes.Length;
        await res.OutputStream.WriteAsync(bytes);
        res.Close();
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _listener?.Close();
        _cts?.Dispose();
    }
}
