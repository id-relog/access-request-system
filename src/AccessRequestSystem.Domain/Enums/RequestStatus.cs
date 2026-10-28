namespace AccessRequestSystem.Domain.Enums;

/// <summary>
/// Статус заявки на доступ к сервису.
/// </summary>
public enum RequestStatus
{
    /// <summary>Заявка подана, ожидает решения администратора.</summary>
    Pending = 0,

    /// <summary>Администратор одобрил доступ.</summary>
    Approved = 1,

    /// <summary>Администратор отклонил заявку.</summary>
    Rejected = 2,

    /// <summary>Одобренный доступ истёк по сроку действия (AccessTo).</summary>
    Expired = 3
}
