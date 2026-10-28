namespace AccessRequestSystem.Domain.Entities;

/// <summary>
/// Запись о том, что прокси-агент проверил доступ сотрудника к домену и что решил.
/// Пишется на каждый запрос AccessCheckController — это и есть реальный лог блокировок.
/// </summary>
public class AccessLog
{
    public Guid Id { get; set; }

    /// <summary>Кто спрашивал (AspNetUsers.Id) — может быть null, если домен спросили без валидной анкеты.</summary>
    public Guid UserId { get; set; }

    public string Domain { get; set; } = string.Empty;

    public bool Allowed { get; set; }

    public DateTime CheckedAt { get; set; }
}
