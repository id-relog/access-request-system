using AccessRequestSystem.Domain.Enums;

namespace AccessRequestSystem.BLL.DTOs;

/// <summary>Данные для создания заявки. Заполняется либо ServiceId, либо CustomServiceName/CustomServiceUrl.</summary>
public record CreateAccessRequestDto(
    Guid? ServiceId,
    string? CustomServiceName,
    string? CustomServiceUrl,
    string? Justification,
    DateOnly AccessFrom,
    DateOnly AccessTo,
    DurationPreset DurationPreset);

/// <summary>Решение администратора по заявке.</summary>
public record DecisionDto(bool Approve, string? Comment);

public record AccessRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeFullName,
    string ServiceName,
    string? ServiceUrl,
    bool IsCustomService,
    string? Justification,
    DateOnly AccessFrom,
    DateOnly AccessTo,
    DurationPreset DurationPreset,
    RequestStatus Status,
    DateTime RequestedAt,
    DateTime? ReviewedAt,
    string? ReviewComment);
