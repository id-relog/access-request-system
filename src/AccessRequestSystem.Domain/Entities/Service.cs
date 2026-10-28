namespace AccessRequestSystem.Domain.Entities;

/// <summary>
/// Встроенный сервис из справочника (например, "Яндекс.Музыка"),
/// который сотрудник может выбрать при создании заявки.
/// </summary>
public class Service
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }

    /// <summary>Домен, извлечённый из Url (например, "music.yandex.by") — по нему прокси-агент сверяет запросы клиента.</summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Дополнительные домены через запятую, которые нужны сайту, но отличаются от основного
    /// (CDN, видео-сервера и т.д.) — например, для YouTube это "googlevideo.com,ytimg.com,ggpht.com".
    /// Поддомены каждого из них (и основного Domain) подходят автоматически.
    /// </summary>
    public string? RelatedDomains { get; set; }

    /// <summary>true — добавлен изначально/системой, false — добавлен администратором вручную.</summary>
    public bool IsBuiltIn { get; set; }

    /// <summary>Позволяет скрыть сервис из формы заявки без физического удаления (есть история заявок).</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public ICollection<AccessRequest> AccessRequests { get; set; } = new List<AccessRequest>();
}
