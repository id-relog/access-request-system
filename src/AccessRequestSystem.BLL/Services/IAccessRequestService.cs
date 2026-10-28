using AccessRequestSystem.BLL.DTOs;
using AccessRequestSystem.Domain.Enums;

namespace AccessRequestSystem.BLL.Services;

public interface IAccessRequestService
{
    Task<AccessRequestDto> CreateAsync(Guid userId, CreateAccessRequestDto dto, CancellationToken ct = default);

    Task<List<AccessRequestDto>> GetMyRequestsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Для администратора: все заявки, опционально отфильтрованные по статусу.</summary>
    Task<List<AccessRequestDto>> GetAllAsync(RequestStatus? status, CancellationToken ct = default);

    /// <summary>Администратор одобряет или отклоняет заявку — меняет Status в БД и пишет запись в историю.</summary>
    Task<AccessRequestDto> DecideAsync(Guid requestId, Guid adminUserId, DecisionDto dto, CancellationToken ct = default);

    /// <summary>Переводит все одобренные, но просроченные заявки в статус Expired. Вызывается фоновой задачей раз в сутки.</summary>
    Task<int> ExpireOutdatedAsync(CancellationToken ct = default);
}
