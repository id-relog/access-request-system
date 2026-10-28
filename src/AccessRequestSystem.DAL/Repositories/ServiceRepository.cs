using AccessRequestSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestSystem.DAL.Repositories;

public class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Service>> GetActiveAsync(CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(ct);
}
