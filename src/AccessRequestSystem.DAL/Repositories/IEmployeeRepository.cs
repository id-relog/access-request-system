using AccessRequestSystem.Domain.Entities;

namespace AccessRequestSystem.DAL.Repositories;

public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
