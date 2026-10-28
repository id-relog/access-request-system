namespace AccessRequestSystem.Domain.Enums;

/// <summary>
/// Быстрый пресет периода доступа для формы заявки.
/// Не влияет на бизнес-логику напрямую — используется только для того,
/// чтобы UI мог предзаполнить AccessFrom/AccessTo и чтобы было видно,
/// каким способом сотрудник выбрал период.
/// </summary>
public enum DurationPreset
{
    /// <summary>Сотрудник указал произвольные даты вручную.</summary>
    Custom = 0,

    Week = 1,
    Month = 2,
    Quarter = 3,
    HalfYear = 4,
    Year = 5
}
