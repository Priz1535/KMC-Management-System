using KMCEventClient.Models;
using KMCEventClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace KMCEventClient.Controllers;

public class RegistrationsController : Controller
{
    private readonly KmcApiService _api;

    public RegistrationsController(KmcApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int eventId)
    {
        try
        {
            var eventItem = await _api.GetEventAsync(eventId);
            if (eventItem is null)
            {
                return NotFound();
            }

            if (eventItem.AvailablePlaces <= 0)
            {
                TempData["Error"] = "This event is currently full.";
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            return View(new RegistrationFormViewModel
            {
                EventID = eventId,
                EventName = eventItem.EventName
            });
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The API could not be reached.";
            return RedirectToAction("Index", "Events");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegistrationFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _api.RegisterForEventAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = "Your event registration was completed successfully.";
            return RedirectToAction("Details", "Events", new { id = model.EventID });
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The API could not be reached.");
            return View(model);
        }
    }
}
