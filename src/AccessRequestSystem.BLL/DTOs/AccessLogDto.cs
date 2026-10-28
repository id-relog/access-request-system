namespace AccessRequestSystem.BLL.DTOs;

public record AccessLogDto(
    Guid Id,
    Guid UserId,
    string? UserEmail,
    string? EmployeeFullName,
    string Domain,
    bool Allowed,
    DateTime CheckedAt);
