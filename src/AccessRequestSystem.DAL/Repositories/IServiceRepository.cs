using AccessRequestSystem.Domain.Entities;

namespace AccessRequestSystem.DAL.Repositories;

public interface IServiceRepository : IRepository<Service>
{
    /// <summary>Активные сервисы для отображения в форме заявки.</summary>
    Task<List<Service>> GetActiveAsync(CancellationToken ct = default);
}
