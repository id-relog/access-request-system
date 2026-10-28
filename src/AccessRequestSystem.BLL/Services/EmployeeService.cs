using AccessRequestSystem.BLL.DTOs;
using AccessRequestSystem.DAL.Repositories;
using AccessRequestSystem.Domain.Entities;

namespace AccessRequestSystem.BLL.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employees;

    public EmployeeService(IEmployeeRepository employees)
    {
        _employees = employees;
    }

    public async Task<EmployeeDto?> GetMyProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var employee = await _employees.GetByUserIdAsync(userId, ct);
        return employee is null ? null : ToDto(employee);
    }

    public async Task<EmployeeDto> UpsertProfileAsync(Guid userId, UpsertEmployeeDto dto, CancellationToken ct = default)
    {
        var employee = await _employees.GetByUserIdAsync(userId, ct);

        if (employee is null)
        {
            employee = new Employee
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            Apply(employee, dto);
            await _employees.AddAsync(employee, ct);
        }
        else
        {
            Apply(employee, dto);
            employee.UpdatedAt = DateTime.UtcNow;
            _employees.Update(employee);
        }

        await _employees.SaveChangesAsync(ct);
        return ToDto(employee);
    }

    private static void Apply(Employee employee, UpsertEmployeeDto dto)
    {
        employee.LastName = dto.LastName;
        employee.FirstName = dto.FirstName;
        employee.MiddleName = dto.MiddleName;
        employee.Position = dto.Position;
        employee.Department = dto.Department;
        employee.Organization = dto.Organization;
        employee.PersonnelNumber = dto.PersonnelNumber;
        employee.Phone = dto.Phone;
        employee.Email = dto.Email;
    }

    private static EmployeeDto ToDto(Employee e) => new(
        e.Id, e.LastName, e.FirstName, e.MiddleName, e.Position,
        e.Department, e.Organization, e.PersonnelNumber, e.Phone, e.Email);
}
