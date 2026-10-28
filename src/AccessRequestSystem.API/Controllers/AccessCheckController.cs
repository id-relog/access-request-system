using System.Security.Claims;
using AccessRequestSystem.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestSystem.API.Controllers;

/// <summary>
/// Эндпоинт, который дёргает прокси-агент (Titanium.Web.Proxy) на каждый запрос клиента:
/// "можно ли этому сотруднику ходить на этот домен прямо сейчас". Источник истины — таблица
/// AccessRequests (AccessProvisioningService), каждая проверка пишется в AccessLogs.
/// </summary>
[ApiController]
[Route("api/access")]
[Authorize]
public class AccessCheckController : ControllerBase
{
    private readonly IEmployeeService _employees;
    private readonly IProvisioningService _provisioning;
    private readonly IAccessLogService _logs;
    private readonly ILogger<AccessCheckController> _logger;

    public AccessCheckController(
        IEmployeeService employees,
        IProvisioningService provisioning,
        IAccessLogService logs,
        ILogger<AccessCheckController> logger)
    {
        _employees = employees;
        _provisioning = provisioning;
        _logs = logs;
        _logger = logger;
    }

    [HttpGet("check")]
    public async Task<IActionResult> CheckAccess([FromQuery] string domain, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return BadRequest("Не указан домен.");

        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var normalizedDomain = domain.Trim().ToLowerInvariant();

       
        var profile = await _employees.GetMyProfileAsync(userId, ct);
        var hasAccess = profile is not null && await _provisioning.HasAccessAsync(normalizedDomain, profile.Id, ct);

        await _logs.LogAsync(userId, normalizedDomain, hasAccess, ct);
        _logger.LogInformation("[access-check] {Domain} -> {Result} (user {UserId})",
            normalizedDomain, hasAccess ? "ALLOW" : "DENY", userId);

        return Ok(new { domain = normalizedDomain, hasAccess });
    }
}
