using System.Linq.Expressions;

namespace AccessRequestSystem.DAL.Repositories;

/// <summary>
/// Базовый обобщённый репозиторий с CRUD-операциями поверх EF Core.
/// Конкретные репозитории (IEmployeeRepository и т.д.) наследуют его
/// и добавляют доменно-специфичные методы выборки.
/// </summary>
public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<List<TEntity>> GetAllAsync(CancellationToken ct = default);

    Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    Task AddAsync(TEntity entity, CancellationToken ct = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    /// <summary>Сохраняет изменения в БД. Вызывается один раз после всех операций в рамках юнита работы (обычно из BLL-сервиса).</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
