using AccessRequestSystem.BLL.DTOs;

namespace AccessRequestSystem.BLL.Services;

public interface IEmployeeService
{
    /// <summary>Возвращает анкету текущего пользователя, либо null, если он её ещё не заполнял.</summary>
    Task<EmployeeDto?> GetMyProfileAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Создаёт анкету, если её ещё нет, либо обновляет существующую.</summary>
    Task<EmployeeDto> UpsertProfileAsync(Guid userId, UpsertEmployeeDto dto, CancellationToken ct = default);
}
