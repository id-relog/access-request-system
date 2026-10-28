using AccessRequestSystem.BLL.DTOs;
using AccessRequestSystem.DAL.Repositories;
using AccessRequestSystem.Domain.Entities;

namespace AccessRequestSystem.BLL.Services;

public interface IAccessLogService
{
    /// <summary>Пишет одну запись лога (запрос прокси-агента к AccessCheckController).</summary>
    Task LogAsync(Guid userId, string domain, bool allowed, CancellationToken ct = default);

    /// <summary>Последние N записей — для страницы администратора, с опциональным поиском.</summary>
    Task<List<AccessLogDto>> GetRecentAsync(int count = 200, string? search = null, CancellationToken ct = default);
}

public class AccessLogService : IAccessLogService
{
    private readonly IAccessLogRepository _logs;

    public AccessLogService(IAccessLogRepository logs)
    {
        _logs = logs;
    }

    public async Task LogAsync(Guid userId, string domain, bool allowed, CancellationToken ct = default)
    {
        _logs.Add(new AccessLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Domain = domain,
            Allowed = allowed,
            CheckedAt = DateTime.UtcNow
        });

        await _logs.SaveChangesAsync(ct);
    }

    public async Task<List<AccessLogDto>> GetRecentAsync(int count = 200, string? search = null, CancellationToken ct = default)
    {
        var rows = await _logs.GetRecentAsync(count, search, ct);
        return rows.Select(r => new AccessLogDto(
            r.Log.Id,
            r.Log.UserId,
            r.Email,
            r.EmployeeFullName,
            r.Log.Domain,
            r.Log.Allowed,
            r.Log.CheckedAt)).ToList();
    }
}
