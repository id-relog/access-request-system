namespace AccessRequestSystem.BLL.DTOs;

public record EmployeeDto(
    Guid Id,
    string LastName,
    string FirstName,
    string? MiddleName,
    string Position,
    string Department,
    string Organization,
    string? PersonnelNumber,
    string? Phone,
    string Email);

/// <summary>Данные, которые сотрудник заполняет/редактирует в своей анкете.</summary>
public record UpsertEmployeeDto(
    string LastName,
    string FirstName,
    string? MiddleName,
    string Position,
    string Department,
    string Organization,
    string? PersonnelNumber,
    string? Phone,
    string Email);
