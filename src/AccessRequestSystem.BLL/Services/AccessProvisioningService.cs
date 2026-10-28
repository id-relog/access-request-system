using AccessRequestSystem.DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace AccessRequestSystem.BLL.Services;

/// <summary>
/// Реальная проверка доступа: смотрит напрямую в таблицу AccessRequests — есть ли
/// у сотрудника одобренная и ещё не истёкшая заявка на этот домен. Это и есть
/// источник истины, которым пользуется прокси-агент (через AccessCheckController).
/// GrantAccessAsync/RevokeAccessAsync ничего не хранят отдельно — статус и период
/// уже лежат в самой заявке, поэтому это просто хуки для логирования.
/// </summary>
public class AccessProvisioningService : IProvisioningService
{
    private readonly IAccessRequestRepository _requests;
    private readonly ILogger<AccessProvisioningService> _logger;

    public AccessProvisioningService(IAccessRequestRepository requests, ILogger<AccessProvisioningService> logger)
    {
        _requests = requests;
        _logger = logger;
    }

    public Task GrantAccessAsync(string domain, Guid employeeId, CancellationToken ct = default)
    {
        _logger.LogInformation("[provisioning] выдан доступ: сотрудник {EmployeeId} → {Domain}", employeeId, domain);
        return Task.CompletedTask;
    }

    public Task RevokeAccessAsync(string domain, Guid employeeId, CancellationToken ct = default)
    {
        _logger.LogInformation("[provisioning] отозван доступ: сотрудник {EmployeeId} → {Domain}", employeeId, domain);
        return Task.CompletedTask;
    }

    public async Task<bool> HasAccessAsync(string domain, Guid employeeId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var normalized = domain.Trim().ToLowerInvariant();
        return await _requests.HasActiveApprovedAccessAsync(employeeId, normalized, today, ct);
    }
}
