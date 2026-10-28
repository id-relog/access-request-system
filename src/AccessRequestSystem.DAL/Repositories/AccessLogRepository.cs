using AccessRequestSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestSystem.DAL.Repositories;

public class AccessLogRepository : IAccessLogRepository
{
    private readonly AppDbContext _context;

    public AccessLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(AccessLog log)
        => _context.Set<AccessLog>().Add(log);

    public async Task<List<(AccessLog Log, string? Email, string? EmployeeFullName)>> GetRecentAsync(
        int count,
        string? search = null,
        CancellationToken ct = default)
    {
        var query =
            from log in _context.Set<AccessLog>().AsNoTracking()
            join user in _context.Users.AsNoTracking() on log.UserId equals user.Id
            join emp in _context.Employees.AsNoTracking() on user.Id equals emp.UserId into empJoin
            from emp in empJoin.DefaultIfEmpty()
            select new
            {
                Log = log,
                Email = user.Email,
                Employee = emp
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(x =>
                (x.Email != null && x.Email.ToLower().Contains(term)) ||
                (x.Employee != null && (
                    x.Employee.LastName.ToLower().Contains(term) ||
                    x.Employee.FirstName.ToLower().Contains(term) ||
                    (x.Employee.MiddleName != null && x.Employee.MiddleName.ToLower().Contains(term)) ||
                    x.Employee.Position.ToLower().Contains(term) ||
                    x.Employee.Department.ToLower().Contains(term) ||
                    x.Employee.Organization.ToLower().Contains(term) ||
                    (x.Employee.PersonnelNumber != null && x.Employee.PersonnelNumber.ToLower().Contains(term)) ||
                    (x.Employee.Phone != null && x.Employee.Phone.ToLower().Contains(term)) ||
                    x.Employee.Email.ToLower().Contains(term)
                )));
        }

        var rows = await query
            .OrderByDescending(x => x.Log.CheckedAt)
            .Take(count)
            .ToListAsync(ct);

        return rows.Select(x =>
        {
            string? fullName = null;
            if (x.Employee is not null)
            {
                fullName = string.Join(' ', new[]
                {
                    x.Employee.LastName,
                    x.Employee.FirstName,
                    x.Employee.MiddleName
                }.Where(p => !string.IsNullOrWhiteSpace(p)));
            }

            string? email = x.Email;
            return ((AccessLog Log, string? Email, string? EmployeeFullName))
                (x.Log, email, string.IsNullOrWhiteSpace(fullName) ? null : fullName);
        }).ToList();
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await _context.SaveChangesAsync(ct);
}
