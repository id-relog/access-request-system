using AccessRequestSystem.Domain.Entities;
using AccessRequestSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestSystem.DAL.Repositories;

public class AccessRequestRepository : Repository<AccessRequest>, IAccessRequestRepository
{
    public AccessRequestRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<AccessRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(r => r.Employee)
            .Include(r => r.Service)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<List<AccessRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(r => r.Service)
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(ct);

    public async Task<List<AccessRequest>> GetAllWithDetailsAsync(RequestStatus? status = null, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking()
            .Include(r => r.Employee)
            .Include(r => r.Service)
            .AsQueryable();

        if (status is not null)
            query = query.Where(r => r.Status == status);

        return await query.OrderByDescending(r => r.RequestedAt).ToListAsync(ct);
    }

    public async Task<List<AccessRequest>> GetApprovedButExpiredAsync(DateOnly today, CancellationToken ct = default)
        => await DbSet
            .Where(r => r.Status == RequestStatus.Approved && r.AccessTo < today)
            .ToListAsync(ct);

    public void AddHistoryEntry(RequestHistory history)
        => Context.Set<RequestHistory>().Add(history);

    public async Task<bool> HasActiveApprovedAccessAsync(Guid employeeId, string domain, DateOnly today, CancellationToken ct = default)
    {
        // Кандидатов на одного сотрудника всегда немного (единицы-десятки активных заявок),
        // поэтому проще и надёжнее забрать их и сравнить домены в памяти, чем городить
        // разбор строки со списком через запятую прямо в SQL.
        var candidates = await DbSet.AsNoTracking()
            .Where(r => r.EmployeeId == employeeId
                        && r.Status == RequestStatus.Approved
                        && r.AccessFrom <= today && r.AccessTo >= today)
            .Select(r => new
            {
                r.CustomServiceDomain,
                ServiceDomain = r.Service != null ? r.Service.Domain : null,
                RelatedDomains = r.Service != null ? r.Service.RelatedDomains : null
            })
            .ToListAsync(ct);

        foreach (var c in candidates)
        {
            if (DomainMatches(domain, c.ServiceDomain) || DomainMatches(domain, c.CustomServiceDomain))
                return true;

            if (string.IsNullOrWhiteSpace(c.RelatedDomains))
                continue;

            foreach (var related in c.RelatedDomains.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (DomainMatches(domain, related))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// true, если requested — это ровно approved, либо его поддомен (например, "www.youtube.com"
    /// подходит под одобренный "youtube.com", а "evilyoutube.com" — НЕ подходит).
    /// </summary>
    internal static bool DomainMatches(string requested, string? approved)
    {
        if (string.IsNullOrWhiteSpace(approved))
            return false;

        approved = approved.Trim().ToLowerInvariant();
        return requested == approved || requested.EndsWith("." + approved, StringComparison.Ordinal);
    }
}
