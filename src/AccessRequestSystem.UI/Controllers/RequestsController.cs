using AccessRequestSystem.UI.Models;
using AccessRequestSystem.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestSystem.UI.Controllers;

[Authorize]
public class RequestsController : Controller
{
    private readonly IApiClient _api;

    public RequestsController(IApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CreateAccessRequestViewModel
        {
            AvailableServices = await _api.GetActiveServicesAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAccessRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableServices = await _api.GetActiveServicesAsync();
            return View(model);
        }

        try
        {
            var body = new CreateAccessRequestBody(
                model.ServiceId == Guid.Empty ? null : model.ServiceId,
                string.IsNullOrWhiteSpace(model.CustomServiceName) ? null : model.CustomServiceName,
                string.IsNullOrWhiteSpace(model.CustomServiceUrl) ? null : model.CustomServiceUrl,
                model.Justification,
                model.AccessFrom,
                model.AccessTo,
                model.DurationPreset);

            await _api.CreateRequestAsync(body);
            TempData["Success"] = "Заявка отправлена на рассмотрение администратору.";
            return RedirectToAction(nameof(My));
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.AvailableServices = await _api.GetActiveServicesAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> My()
    {
        var requests = await _api.GetMyRequestsAsync();
        return View(requests);
    }
}
