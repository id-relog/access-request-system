using AccessRequestSystem.API.Auth;
using AccessRequestSystem.BLL.DTOs;
using AccessRequestSystem.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestSystem.API.Controllers;

[ApiController]
[Route("api/access-logs")]
[Authorize(Roles = Roles.Administrator)]
public class AccessLogsController : ControllerBase
{
    private readonly IAccessLogService _logs;

    public AccessLogsController(IAccessLogService logs)
    {
        _logs = logs;
    }

    /// <summary>
    /// Последние проверки доступа. <paramref name="q"/> — поиск по email / ФИО / должности / подразделению и т.д.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<AccessLogDto>>> GetRecent(
        [FromQuery] int count = 200,
        [FromQuery] string? q = null,
        CancellationToken ct = default)
        => Ok(await _logs.GetRecentAsync(count, q, ct));
}
