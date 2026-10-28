namespace AccessRequestSystem.UI.Models;

public record AccessLogDto(
    Guid Id,
    Guid UserId,
    string? UserEmail,
    string? EmployeeFullName,
    string Domain,
    bool Allowed,
    DateTime CheckedAt);
