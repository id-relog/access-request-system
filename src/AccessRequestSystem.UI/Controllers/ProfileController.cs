using AccessRequestSystem.UI.Models;
using AccessRequestSystem.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestSystem.UI.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IApiClient _api;

    public ProfileController(IApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var profile = await _api.GetMyProfileAsync();

        var model = profile is null
            ? new EmployeeFormViewModel()
            : new EmployeeFormViewModel
            {
                LastName = profile.LastName,
                FirstName = profile.FirstName,
                MiddleName = profile.MiddleName,
                Position = profile.Position,
                Department = profile.Department,
                Organization = profile.Organization,
                PersonnelNumber = profile.PersonnelNumber,
                Phone = profile.Phone,
                Email = profile.Email
            };

        ViewBag.IsNew = profile is null;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmployeeFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _api.UpsertProfileAsync(model);
            TempData["Success"] = "Анкета сохранена.";
            return RedirectToAction("My", "Requests");
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
}
