using AccessRequestSystem.Domain.Entities;
using AccessRequestSystem.Domain.Enums;

namespace AccessRequestSystem.DAL.Repositories;

public interface IAccessRequestRepository : IRepository<AccessRequest>
{
    /// <summary>Заявка с подгруженными Employee и Service — для карточки заявки/решения администратора.</summary>
    Task<AccessRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>Все заявки конкретного сотрудника, свежие сверху.</summary>
    Task<List<AccessRequest>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct = default);

    /// <summary>Список заявок для администратора с подгруженными Employee/Service, с опциональным фильтром по статусу.</summary>
    Task<List<AccessRequest>> GetAllWithDetailsAsync(RequestStatus? status = null, CancellationToken ct = default);

    /// <summary>Одобренные заявки, у которых срок доступа уже истёк, но статус ещё не переведён в Expired — для фоновой задачи.</summary>
    Task<List<AccessRequest>> GetApprovedButExpiredAsync(DateOnly today, CancellationToken ct = default);

    /// <summary>
    /// Явно добавляет новую запись истории в контекст (Added), не полагаясь на автоопределение
    /// состояния через навигационное свойство — у RequestHistory ключ выставляется на клиенте,
    /// и EF не всегда сам понимает, что это новая запись, если она попала в трекер только через
    /// AccessRequest.History.
    /// </summary>
    void AddHistoryEntry(RequestHistory history);

    /// <summary>Есть ли у сотрудника действующий (одобренный и не истёкший) доступ к этому домену — для прокси-агента.</summary>
    Task<bool> HasActiveApprovedAccessAsync(Guid employeeId, string domain, DateOnly today, CancellationToken ct = default);
}
