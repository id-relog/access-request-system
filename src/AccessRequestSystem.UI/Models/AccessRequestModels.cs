using System.ComponentModel.DataAnnotations;

namespace AccessRequestSystem.UI.Models;

/// <summary>То, что возвращает API по заявке.</summary>
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

/// <summary>Тело запроса POST /api/requests.</summary>
public record CreateAccessRequestBody(
    Guid? ServiceId,
    string? CustomServiceName,
    string? CustomServiceUrl,
    string? Justification,
    DateOnly AccessFrom,
    DateOnly AccessTo,
    DurationPreset DurationPreset);

/// <summary>Форма создания заявки — сотрудник либо выбирает сервис из списка, либо вводит свой.</summary>
public class CreateAccessRequestViewModel : IValidatableObject
{
    [Display(Name = "Сервис из списка")]
    public Guid? ServiceId { get; set; }

    [Display(Name = "Название сервиса (если его нет в списке)")]
    public string? CustomServiceName { get; set; }

    [Display(Name = "Ссылка на сервис")]
    [Url(ErrorMessage = "Некорректная ссылка")]
    public string? CustomServiceUrl { get; set; }

    [Display(Name = "Обоснование — зачем нужен доступ")]
    public string? Justification { get; set; }

    [Required(ErrorMessage = "Укажите дату начала доступа")]
    [DataType(DataType.Date)]
    [Display(Name = "Доступ с")]
    public DateOnly AccessFrom { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "Укажите дату окончания доступа")]
    [DataType(DataType.Date)]
    [Display(Name = "Доступ по")]
    public DateOnly AccessTo { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddMonths(1));

    [Display(Name = "Период")]
    public DurationPreset DurationPreset { get; set; } = DurationPreset.Month;

    public List<ServiceDto> AvailableServices { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var hasService = ServiceId is not null && ServiceId != Guid.Empty;
        var hasCustom = !string.IsNullOrWhiteSpace(CustomServiceName);

        if (hasService == hasCustom)
            yield return new ValidationResult(
                "Выберите сервис из списка ЛИБО укажите название своего сервиса — не оба варианта сразу и не оставляйте оба пустыми.",
                new[] { nameof(ServiceId), nameof(CustomServiceName) });

        if (AccessTo <= AccessFrom)
            yield return new ValidationResult(
                "Дата окончания доступа должна быть позже даты начала.",
                new[] { nameof(AccessTo) });
    }
}

/// <summary>Тело запроса PUT /api/requests/{id}/decision.</summary>
public record DecisionBody(bool Approve, string? Comment);

public class DecisionViewModel
{
    public Guid RequestId { get; set; }
    public bool Approve { get; set; }

    [Display(Name = "Комментарий")]
    public string? Comment { get; set; }
}
