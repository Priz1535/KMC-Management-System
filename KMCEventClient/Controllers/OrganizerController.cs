using KMCEventClient.Models;
using KMCEventClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace KMCEventClient.Controllers;

public class OrganizerController : Controller
{
    private readonly KmcApiService _api;

    public OrganizerController(KmcApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Dashboard()
    {
        var token = GetToken();
        if (token is null)
        {
            return RedirectToLogin();
        }

        try
        {
            ViewBag.OrganizerName = HttpContext.Session.GetString("OrganizerName");
            ViewBag.OrganizationName = HttpContext.Session.GetString("OrganizationName");
            return View(await _api.GetMyEventsAsync(token));
        }
        catch (HttpRequestException)
        {
            ViewBag.ErrorMessage = "The API could not be reached.";
            return View(new List<EventViewModel>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> CreateEvent()
    {
        if (GetToken() is null)
        {
            return RedirectToLogin();
        }

        return View(new EventFormViewModel
        {
            EventDate = DateTime.Today.AddDays(7),
            EventTime = new TimeSpan(9, 0, 0),
            Capacity = 50,
            Venues = await SafeGetVenuesAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventFormViewModel model)
    {
        var token = GetToken();
        if (token is null)
        {
            return RedirectToLogin();
        }

        if (!ModelState.IsValid)
        {
            model.Venues = await SafeGetVenuesAsync();
            return View(model);
        }

        try
        {
            var result = await _api.CreateEventAsync(model, token);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model.Venues = await SafeGetVenuesAsync();
                return View(model);
            }

            TempData["Success"] = "Event created successfully.";
            return RedirectToAction(nameof(Dashboard));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The API could not be reached.");
            model.Venues = await SafeGetVenuesAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        if (GetToken() is null)
        {
            return RedirectToLogin();
        }

        try
        {
            var item = await _api.GetEventAsync(id);
            if (item is null)
            {
                return NotFound();
            }

            if (item.OrganizerID != HttpContext.Session.GetInt32("OrganizerID"))
            {
                TempData["Error"] = "You can only edit events that you created.";
                return RedirectToAction(nameof(Dashboard));
            }

            return View(new EventFormViewModel
            {
                EventID = item.EventID,
                EventName = item.EventName,
                Description = item.Description,
                EventDate = item.EventDate,
                EventTime = item.EventTime,
                EventType = item.EventType,
                Capacity = item.Capacity,
                VenueID = item.VenueID,
                Venues = await SafeGetVenuesAsync()
            });
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The API could not be reached.";
            return RedirectToAction(nameof(Dashboard));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(EventFormViewModel model)
    {
        var token = GetToken();
        if (token is null)
        {
            return RedirectToLogin();
        }

        if (!ModelState.IsValid)
        {
            model.Venues = await SafeGetVenuesAsync();
            return View(model);
        }

        try
        {
            var result = await _api.UpdateEventAsync(model, token);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                model.Venues = await SafeGetVenuesAsync();
                return View(model);
            }

            TempData["Success"] = "Event updated successfully.";
            return RedirectToAction(nameof(Dashboard));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The API could not be reached.");
            model.Venues = await SafeGetVenuesAsync();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        if (GetToken() is null)
        {
            return RedirectToLogin();
        }

        try
        {
            var item = await _api.GetEventAsync(id);
            if (item is null)
            {
                return NotFound();
            }

            if (item.OrganizerID != HttpContext.Session.GetInt32("OrganizerID"))
            {
                TempData["Error"] = "You can only delete events that you created.";
                return RedirectToAction(nameof(Dashboard));
            }

            return View(item);
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The API could not be reached.";
            return RedirectToAction(nameof(Dashboard));
        }
    }

    [HttpPost, ActionName("DeleteEvent")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEventConfirmed(int id)
    {
        var token = GetToken();
        if (token is null)
        {
            return RedirectToLogin();
        }

        try
        {
            var result = await _api.DeleteEventAsync(id, token);
            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(Dashboard));
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The API could not be reached.";
            return RedirectToAction(nameof(Dashboard));
        }
    }

    public async Task<IActionResult> Registrations(int eventId)
    {
        var token = GetToken();
        if (token is null)
        {
            return RedirectToLogin();
        }

        try
        {
            var eventItem = await _api.GetEventAsync(eventId);
            ViewBag.EventName = eventItem?.EventName ?? "Event";

            var result = await _api.GetEventRegistrationsAsync(eventId, token);
            if (!result.Success)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Dashboard));
            }

            return View(result.Data ?? new List<RegistrationViewModel>());
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The API could not be reached.";
            return RedirectToAction(nameof(Dashboard));
        }
    }

    private string? GetToken() => HttpContext.Session.GetString("AuthToken");

    private IActionResult RedirectToLogin()
    {
        TempData["Error"] = "Please log in as an event organizer first.";
        return RedirectToAction("Login", "Account");
    }

    private async Task<List<VenueViewModel>> SafeGetVenuesAsync()
    {
        try
        {
            return await _api.GetVenuesAsync();
        }
        catch (HttpRequestException)
        {
            return new List<VenueViewModel>();
        }
    }
}
