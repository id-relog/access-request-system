using AccessRequestSystem.Domain.Entities;

namespace AccessRequestSystem.DAL.Repositories;

public interface IAccessLogRepository
{
    void Add(AccessLog log);

    /// <summary>
    /// Последние записи лога с email (AspNetUsers) и ФИО из анкеты (Employees).
    /// <paramref name="search"/> — фильтр по email / ФИО / должности / подразделению / организации / телефону / табельному.
    /// </summary>
    Task<List<(AccessLog Log, string? Email, string? EmployeeFullName)>> GetRecentAsync(
        int count,
        string? search = null,
        CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
