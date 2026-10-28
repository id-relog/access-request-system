using AccessRequestSystem.Domain.Enums;

namespace AccessRequestSystem.Domain.Entities;

/// <summary>
/// Заявка сотрудника на доступ к сервису — либо выбранному из справочника (ServiceId),
/// либо указанному вручную (CustomServiceName/CustomServiceUrl). Доступ всегда выдаётся
/// на ограниченный период [AccessFrom; AccessTo], а не бессрочно.
/// </summary>
public class AccessRequest
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    /// <summary>Заполняется, если сервис выбран из справочника.</summary>
    public Guid? ServiceId { get; set; }
    public Service? Service { get; set; }

    /// <summary>Заполняется, если сервиса нет в справочнике и сотрудник ввёл его сам.</summary>
    public string? CustomServiceName { get; set; }
    public string? CustomServiceUrl { get; set; }

    /// <summary>Домен, извлечённый из CustomServiceUrl — для сверки прокси-агентом (симметрично Service.Domain).</summary>
    public string? CustomServiceDomain { get; set; }

    /// <summary>Обоснование от сотрудника — зачем нужен доступ.</summary>
    public string? Justification { get; set; }

    /// <summary>Дата начала запрашиваемого периода доступа.</summary>
    public DateOnly AccessFrom { get; set; }

    /// <summary>Дата окончания запрашиваемого периода доступа (должна быть строго позже AccessFrom).</summary>
    public DateOnly AccessTo { get; set; }

    /// <summary>Как сотрудник выбрал период в UI — на бизнес-логику не влияет, только для истории/UI.</summary>
    public DurationPreset DurationPreset { get; set; } = DurationPreset.Custom;

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public DateTime RequestedAt { get; set; }

    /// <summary>Кто из администраторов принял решение (AspNetUsers.Id).</summary>
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewComment { get; set; }

    public ICollection<RequestHistory> History { get; set; } = new List<RequestHistory>();

    /// <summary>true, если сервис не из справочника, а введён сотрудником вручную.</summary>
    public bool IsCustomService => ServiceId is null;

    /// <summary>Домен, который реально сверяет прокси-агент — из справочника либо введённый вручную.</summary>
    public string? EffectiveDomain => Service?.Domain ?? CustomServiceDomain;
}
