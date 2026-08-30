using KMCEventClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace KMCEventClient.Controllers;

public class EventsController : Controller
{
    private readonly KmcApiService _api;

    public EventsController(KmcApiService api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(DateTime? date, string? type)
    {
        ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd") ?? string.Empty;
        ViewBag.SelectedType = type ?? string.Empty;

        try
        {
            return View(await _api.GetEventsAsync(date, type));
        }
        catch (HttpRequestException)
        {
            ViewBag.ErrorMessage = "The event service is currently unavailable. Make sure the API project is running.";
            return View(new List<Models.EventViewModel>());
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var item = await _api.GetEventAsync(id);
            return item is null ? NotFound() : View(item);
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The API could not be reached.";
            return RedirectToAction(nameof(Index));
        }
    }
}
