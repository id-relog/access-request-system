using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace AccessRequestSystem.ProxyAgent;

public record ProxyUserSession(string Token, DateTime ExpiresAt, string PasswordHash, bool SsoDone);

/// <summary>Общее состояние аутентификации прокси (кэш + смена realm при invalidate).</summary>
public class ProxyAuthState
{
    private readonly ILogger<ProxyAuthState> _logger;
    private readonly ConcurrentDictionary<string, ProxyUserSession> _sessions = new(StringComparer.OrdinalIgnoreCase);
    private string _realm = "Access Request — https://localhost:7050";
    private Action<string>? _onRealmChanged;

    public ProxyAuthState(ILogger<ProxyAuthState> logger)
    {
        _logger = logger;
    }

    public string Realm => _realm;

    public void SetRealmChangedHandler(Action<string> handler)
        => _onRealmChanged = handler;

    public bool TryGetSession(string username, out ProxyUserSession session)
        => _sessions.TryGetValue(username, out session!);

    public void SetSession(string username, ProxyUserSession session)
        => _sessions[username] = session;

    public void MarkSsoDone(string username)
    {
        if (_sessions.TryGetValue(username, out var s))
            _sessions[username] = s with { SsoDone = true };
    }

    public void InvalidateAll(string reason)
    {
        _sessions.Clear();
        _realm = $"Access Request — https://localhost:7050 [{Guid.NewGuid():N}]";
        _logger.LogWarning("Прокси-сессии сброшены ({Reason}). Новый realm: {Realm}", reason, _realm);
        _onRealmChanged?.Invoke(_realm);
    }
}
