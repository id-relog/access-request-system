namespace AccessRequestSystem.Domain.Entities;

/// <summary>
/// Анкета сотрудника. Привязана 1-к-1 к учётной записи Identity (UserId).
/// </summary>
public class Employee
{
    public Guid Id { get; set; }

    /// <summary>Ссылка на AspNetUsers.Id (Identity). Один пользователь — одна анкета.</summary>
    public Guid UserId { get; set; }

    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }

    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;

    public string? PersonnelNumber { get; set; }
    public string? Phone { get; set; }
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<AccessRequest> AccessRequests { get; set; } = new List<AccessRequest>();
}
