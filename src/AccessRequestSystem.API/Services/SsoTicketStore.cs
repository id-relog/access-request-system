using System.Collections.Concurrent;

namespace AccessRequestSystem.API.Services;

public record SsoTicketPayload(
    Guid UserId,
    string Email,
    string Token,
    DateTime ExpiresAt,
    IReadOnlyList<string> Roles);

public interface ISsoTicketStore
{
    string Create(SsoTicketPayload payload, TimeSpan? lifetime = null);
    SsoTicketPayload? Consume(string ticket);
}

/// <summary>Одноразовые SSO-билеты: прокси → UI (вход без повторного пароля).</summary>
public class SsoTicketStore : ISsoTicketStore
{
    private readonly ConcurrentDictionary<string, (SsoTicketPayload Payload, DateTime ExpiresAt)> _tickets = new();

    public string Create(SsoTicketPayload payload, TimeSpan? lifetime = null)
    {
        var ticket = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var ttl = lifetime ?? TimeSpan.FromMinutes(2);
        _tickets[ticket] = (payload, DateTime.UtcNow.Add(ttl));
        return ticket;
    }

    public SsoTicketPayload? Consume(string ticket)
    {
        if (string.IsNullOrWhiteSpace(ticket))
            return null;

        if (!_tickets.TryRemove(ticket.Trim(), out var entry))
            return null;

        if (entry.ExpiresAt <= DateTime.UtcNow)
            return null;

        if (entry.Payload.ExpiresAt <= DateTime.UtcNow)
            return null;

        return entry.Payload;
    }
}
