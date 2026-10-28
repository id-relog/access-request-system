using AccessRequestSystem.UI.Models;
using AccessRequestSystem.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestSystem.UI.Controllers;

[Authorize(Roles = Roles.Administrator)]
public class AdminController : Controller
{
    private readonly IApiClient _api;

    public AdminController(IApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Requests(RequestStatus? status)
    {
        ViewBag.SelectedStatus = status;
        var requests = await _api.GetAllRequestsAsync(status);
        return View(requests);
    }

    [HttpGet]
    public async Task<IActionResult> AccessLogs(string? q)
    {
        ViewBag.Search = q;
        var logs = await _api.GetAccessLogsAsync(search: q);
        return View(logs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decide(DecisionViewModel model)
    {
        try
        {
            await _api.DecideAsync(model.RequestId, new DecisionBody(model.Approve, model.Comment));
            TempData["Success"] = model.Approve ? "Заявка одобрена." : "Заявка отклонена.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Requests));
    }

    [HttpGet]
    public async Task<IActionResult> Services()
    {
        var services = await _api.GetAllServicesAsync();
        return View(services);
    }

    [HttpGet]
    public IActionResult CreateService() => View(new ServiceFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateService(ServiceFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _api.CreateServiceAsync(model);
            TempData["Success"] = "Сервис добавлен в справочник.";
            return RedirectToAction(nameof(Services));
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditService(Guid id)
    {
        var service = (await _api.GetAllServicesAsync()).FirstOrDefault(s => s.Id == id);
        if (service is null)
            return NotFound();

        ViewBag.ServiceId = id;
        return View(new ServiceFormViewModel
        {
            Name = service.Name,
            Description = service.Description,
            Url = service.Url,
            RelatedDomains = service.RelatedDomains,
            IsActive = service.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditService(Guid id, ServiceFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ServiceId = id;
            return View(model);
        }

        try
        {
            await _api.UpdateServiceAsync(id, model);
            TempData["Success"] = "Сервис обновлён.";
            return RedirectToAction(nameof(Services));
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.ServiceId = id;
            return View(model);
        }
    }
}
