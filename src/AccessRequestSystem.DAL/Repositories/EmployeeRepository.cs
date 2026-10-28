using AccessRequestSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestSystem.DAL.Repositories;

public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(e => e.UserId == userId, ct);
}
