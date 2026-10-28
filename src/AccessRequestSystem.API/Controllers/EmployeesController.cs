using System.Security.Claims;
using AccessRequestSystem.BLL.DTOs;
using AccessRequestSystem.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestSystem.API.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<EmployeeDto>> GetMe(CancellationToken ct)
    {
        var profile = await _employeeService.GetMyProfileAsync(CurrentUserId, ct);
        return profile is null ? NotFound("Анкета ещё не заполнена.") : Ok(profile);
    }

    [HttpPut("me")]
    public async Task<ActionResult<EmployeeDto>> UpsertMe(UpsertEmployeeDto dto, CancellationToken ct)
        => Ok(await _employeeService.UpsertProfileAsync(CurrentUserId, dto, ct));

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
