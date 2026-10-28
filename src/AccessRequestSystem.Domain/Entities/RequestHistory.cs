using AccessRequestSystem.Domain.Enums;

namespace AccessRequestSystem.Domain.Entities;

/// <summary>
/// Запись истории изменения статуса заявки — для аудита
/// (кто, когда и с каким комментарием менял статус).
/// </summary>
public class RequestHistory
{
    public Guid Id { get; set; }

    public Guid AccessRequestId { get; set; }
    public AccessRequest AccessRequest { get; set; } = null!;

    public RequestStatus OldStatus { get; set; }
    public RequestStatus NewStatus { get; set; }

    /// <summary>AspNetUsers.Id пользователя, который изменил статус (администратор либо системный процесс истечения срока).</summary>
    public Guid ChangedByUserId { get; set; }

    public DateTime ChangedAt { get; set; }
    public string? Comment { get; set; }
}
