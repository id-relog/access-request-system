using AccessRequestSystem.BLL.DTOs;
using AccessRequestSystem.BLL.Exceptions;
using AccessRequestSystem.DAL.Repositories;
using AccessRequestSystem.Domain.Entities;
using AccessRequestSystem.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AccessRequestSystem.BLL.Services;

public class AccessRequestService : IAccessRequestService
{
    private readonly IAccessRequestRepository _requests;
    private readonly IEmployeeRepository _employees;
    private readonly IServiceRepository _services;
    private readonly IProvisioningService _provisioning;
    private readonly ILogger<AccessRequestService> _logger;

    public AccessRequestService(
        IAccessRequestRepository requests,
        IEmployeeRepository employees,
        IServiceRepository services,
        IProvisioningService provisioning,
        ILogger<AccessRequestService> logger)
    {
        _requests = requests;
        _employees = employees;
        _services = services;
        _provisioning = provisioning;
        _logger = logger;
    }

    public async Task<AccessRequestDto> CreateAsync(Guid userId, CreateAccessRequestDto dto, CancellationToken ct = default)
    {
        var employee = await _employees.GetByUserIdAsync(userId, ct)
            ?? throw new BusinessRuleException("Сначала нужно заполнить анкету сотрудника.");

        ValidateServiceSelection(dto);
        ValidatePeriod(dto.AccessFrom, dto.AccessTo);

        Service? service = null;
        if (dto.ServiceId is not null)
        {
            service = await _services.GetByIdAsync(dto.ServiceId.Value, ct)
                ?? throw new NotFoundException($"Сервис {dto.ServiceId} не найден.");

            if (!service.IsActive)
                throw new BusinessRuleException("Выбранный сервис больше не доступен для заявок.");
        }

        var request = new AccessRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            ServiceId = dto.ServiceId,
            CustomServiceName = dto.ServiceId is null ? dto.CustomServiceName : null,
            CustomServiceUrl = dto.ServiceId is null ? dto.CustomServiceUrl : null,
            CustomServiceDomain = dto.ServiceId is null ? ServiceCatalogService.ExtractDomain(dto.CustomServiceUrl) : null,
            Justification = dto.Justification,
            AccessFrom = dto.AccessFrom,
            AccessTo = dto.AccessTo,
            DurationPreset = dto.DurationPreset,
            Status = RequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        await _requests.AddAsync(request, ct);
        await _requests.SaveChangesAsync(ct);

        request.Employee = employee;
        request.Service = service;
        return ToDto(request);
    }

    public async Task<List<AccessRequestDto>> GetMyRequestsAsync(Guid userId, CancellationToken ct = default)
    {
        var employee = await _employees.GetByUserIdAsync(userId, ct);
        if (employee is null)
            return new List<AccessRequestDto>(); // анкета ещё не заполнена — значит и заявок пока нет

        var requests = await _requests.GetByEmployeeAsync(employee.Id, ct);
        return requests.Select(r => ToDto(r, employee)).ToList();
    }

    public async Task<List<AccessRequestDto>> GetAllAsync(RequestStatus? status, CancellationToken ct = default)
    {
        var requests = await _requests.GetAllWithDetailsAsync(status, ct);
        return requests.Select(r => ToDto(r)).ToList();
    }

    public async Task<AccessRequestDto> DecideAsync(Guid requestId, Guid adminUserId, DecisionDto dto, CancellationToken ct = default)
    {
        var request = await _requests.GetByIdWithDetailsAsync(requestId, ct)
            ?? throw new NotFoundException($"Заявка {requestId} не найдена.");

        if (request.Status != RequestStatus.Pending)
            throw new BusinessRuleException("Решение уже принято по этой заявке — повторное изменение статуса не допускается.");

        var oldStatus = request.Status;
        request.Status = dto.Approve ? RequestStatus.Approved : RequestStatus.Rejected;
        request.ReviewedByUserId = adminUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewComment = dto.Comment;

        var history = new RequestHistory
        {
            Id = Guid.NewGuid(),
            AccessRequestId = request.Id,
            OldStatus = oldStatus,
            NewStatus = request.Status,
            ChangedByUserId = adminUserId,
            ChangedAt = DateTime.UtcNow,
            Comment = dto.Comment
        };
        _requests.AddHistoryEntry(history);

        await _requests.SaveChangesAsync(ct);

        var domain = request.EffectiveDomain;
        if (!string.IsNullOrWhiteSpace(domain))
        {
            if (dto.Approve)
            {
                await _provisioning.GrantAccessAsync(domain, request.EmployeeId, ct);
                _logger.LogInformation("Доступ выдан: сотрудник {EmployeeId}, домен {Domain}, заявка {RequestId}", request.EmployeeId, domain, request.Id);
            }
            // Если отклонили — доступ и не выдавался, ничего физически отзывать не нужно
        }
        else
        {
            _logger.LogWarning("У заявки {RequestId} не удалось определить домен — прокси-агент не сможет её применить физически", request.Id);
        }

        return ToDto(request);
    }

    public async Task<int> ExpireOutdatedAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expired = await _requests.GetApprovedButExpiredAsync(today, ct);

        foreach (var request in expired)
        {
            var oldStatus = request.Status;
            request.Status = RequestStatus.Expired;

            var history = new RequestHistory
            {
                Id = Guid.NewGuid(),
                AccessRequestId = request.Id,
                OldStatus = oldStatus,
                NewStatus = RequestStatus.Expired,
                ChangedByUserId = Guid.Empty,
                ChangedAt = DateTime.UtcNow,
                Comment = "Автоматически: истёк срок действия доступа (AccessTo)."
            };
            _requests.AddHistoryEntry(history);

            // Отзываем физический доступ
            var domain = request.EffectiveDomain;
            if (!string.IsNullOrWhiteSpace(domain))
            {
                try
                {
                    await _provisioning.RevokeAccessAsync(domain, request.EmployeeId, ct);
                    _logger.LogInformation("Доступ отозван по истечению срока: сотрудник {EmployeeId}, домен {Domain}, заявка {RequestId}", request.EmployeeId, domain, request.Id);
                }
                catch (Exception ex)
                {
                    // Логируем, но не прерываем обработку остальных заявок
                    _logger.LogError(ex, "Не удалось отозвать доступ к {Domain} для заявки {RequestId}", domain, request.Id);
                }
            }
        }

        if (expired.Count > 0)
            await _requests.SaveChangesAsync(ct);

        return expired.Count;
    }
    private static void ValidateServiceSelection(CreateAccessRequestDto dto)
    {
        var hasService = dto.ServiceId is not null;
        var hasCustom = !string.IsNullOrWhiteSpace(dto.CustomServiceName);

        if (hasService == hasCustom)
            throw new BusinessRuleException(
                "Нужно либо выбрать сервис из списка, либо указать название своего сервиса — но не оба варианта сразу и не оставлять оба пустыми.");
    }

    private static void ValidatePeriod(DateOnly from, DateOnly to)
    {
        if (to <= from)
            throw new BusinessRuleException("Дата окончания доступа должна быть позже даты начала.");

        if (from < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessRuleException("Дата начала доступа не может быть в прошлом.");
    }

    private static AccessRequestDto ToDto(AccessRequest r, Employee? employeeOverride = null)
    {
        var employee = employeeOverride ?? r.Employee;
        var fullName = $"{employee.LastName} {employee.FirstName} {employee.MiddleName}".Trim();
        var serviceName = r.Service?.Name ?? r.CustomServiceName ?? "—";
        var serviceUrl = r.Service?.Url ?? r.CustomServiceUrl;

        return new AccessRequestDto(
            r.Id, r.EmployeeId, fullName, serviceName, serviceUrl, r.IsCustomService,
            r.Justification, r.AccessFrom, r.AccessTo, r.DurationPreset, r.Status,
            r.RequestedAt, r.ReviewedAt, r.ReviewComment);
    }
}
