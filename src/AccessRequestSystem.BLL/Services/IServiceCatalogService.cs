using AccessRequestSystem.BLL.DTOs;

namespace AccessRequestSystem.BLL.Services;

public interface IServiceCatalogService
{
    /// <summary>Активные сервисы — для формы заявки сотрудника.</summary>
    Task<List<ServiceDto>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Все сервисы, включая неактивные — для панели администратора.</summary>
    Task<List<ServiceDto>> GetAllAsync(CancellationToken ct = default);

    Task<ServiceDto> CreateAsync(UpsertServiceDto dto, CancellationToken ct = default);

    Task<ServiceDto> UpdateAsync(Guid id, UpsertServiceDto dto, CancellationToken ct = default);
}
