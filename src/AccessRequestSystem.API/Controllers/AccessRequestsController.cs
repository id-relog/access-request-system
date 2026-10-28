using System.Security.Claims;
using AccessRequestSystem.API.Auth;
using AccessRequestSystem.BLL.DTOs;
using AccessRequestSystem.BLL.Services;
using AccessRequestSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestSystem.API.Controllers;

[ApiController]
[Route("api/requests")]
[Authorize]
public class AccessRequestsController : ControllerBase
{
    private readonly IAccessRequestService _requests;

    public AccessRequestsController(IAccessRequestService requests)
    {
        _requests = requests;
    }

    [HttpPost]
    public async Task<ActionResult<AccessRequestDto>> Create(CreateAccessRequestDto dto, CancellationToken ct)
        => Ok(await _requests.CreateAsync(CurrentUserId, dto, ct));

    [HttpGet("my")]
    public async Task<ActionResult<List<AccessRequestDto>>> GetMy(CancellationToken ct)
        => Ok(await _requests.GetMyRequestsAsync(CurrentUserId, ct));

    /// <summary>Все заявки — только для администратора, с опциональным фильтром по статусу.</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<List<AccessRequestDto>>> GetAll([FromQuery] RequestStatus? status, CancellationToken ct)
        => Ok(await _requests.GetAllAsync(status, ct));

    /// <summary>Одобрить/отклонить заявку — только для администратора.</summary>
    [HttpPut("{id:guid}/decision")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<AccessRequestDto>> Decide(Guid id, DecisionDto dto, CancellationToken ct)
        => Ok(await _requests.DecideAsync(id, CurrentUserId, dto, ct));

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
