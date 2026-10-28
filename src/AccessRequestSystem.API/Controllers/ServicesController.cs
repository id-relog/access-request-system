using AccessRequestSystem.API.Auth;
using AccessRequestSystem.BLL.DTOs;
using AccessRequestSystem.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestSystem.API.Controllers;

[ApiController]
[Route("api/services")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IServiceCatalogService _catalog;

    public ServicesController(IServiceCatalogService catalog)
    {
        _catalog = catalog;
    }

    /// <summary>Активные сервисы - доступно всем авторизованным (для формы заявки).</summary>
    [HttpGet]
    public async Task<ActionResult<List<ServiceDto>>> GetActive(CancellationToken ct)
        => Ok(await _catalog.GetActiveAsync(ct));

    /// <summary>Все сервисы, включая неактивные - только для администратора.</summary>
    [HttpGet("all")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<List<ServiceDto>>> GetAll(CancellationToken ct)
        => Ok(await _catalog.GetAllAsync(ct));

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<ServiceDto>> Create(UpsertServiceDto dto, CancellationToken ct)
        => Ok(await _catalog.CreateAsync(dto, ct));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<ServiceDto>> Update(Guid id, UpsertServiceDto dto, CancellationToken ct)
        => Ok(await _catalog.UpdateAsync(id, dto, ct));
}
