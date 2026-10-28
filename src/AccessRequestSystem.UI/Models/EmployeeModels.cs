using System.ComponentModel.DataAnnotations;

namespace AccessRequestSystem.UI.Models;

/// <summary>То, что возвращает GET /api/employees/me.</summary>
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

/// <summary>Форма анкеты сотрудника.</summary>
public class EmployeeFormViewModel
{
    [Required(ErrorMessage = "Укажите фамилию")]
    [Display(Name = "Фамилия")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите имя")]
    [Display(Name = "Имя")]
    public string FirstName { get; set; } = string.Empty;

    [Display(Name = "Отчество")]
    public string? MiddleName { get; set; }

    [Required(ErrorMessage = "Укажите должность")]
    [Display(Name = "Должность")]
    public string Position { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите структурное подразделение")]
    [Display(Name = "Структурное подразделение")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите наименование организации")]
    [Display(Name = "Наименование организации")]
    public string Organization { get; set; } = string.Empty;

    [Display(Name = "Табельный номер")]
    public string? PersonnelNumber { get; set; }

    [Phone(ErrorMessage = "Некорректный номер телефона")]
    [Display(Name = "Контактный телефон")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Укажите рабочую почту")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    [Display(Name = "Рабочая почта")]
    public string Email { get; set; } = string.Empty;
}
