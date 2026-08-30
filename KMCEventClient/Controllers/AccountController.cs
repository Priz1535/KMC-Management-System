using KMCEventClient.Models;
using KMCEventClient.Services;
using Microsoft.AspNetCore.Mvc;

namespace KMCEventClient.Controllers;

public class AccountController : Controller
{
    private readonly KmcApiService _api;

    public AccountController(KmcApiService api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _api.LoginAsync(model);
            if (!result.Success || result.Data is null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            HttpContext.Session.SetString("AuthToken", result.Data.Token);
            HttpContext.Session.SetString("OrganizerName", result.Data.Organizer.OrganizerName);
            HttpContext.Session.SetString("OrganizationName", result.Data.Organizer.OrganizationName);
            HttpContext.Session.SetInt32("OrganizerID", result.Data.Organizer.OrganizerID);

            TempData["Success"] = $"Welcome back, {result.Data.Organizer.OrganizerName}.";
            return RedirectToAction("Dashboard", "Organizer");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The API could not be reached. Make sure KMCEventAPI is running.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterOrganizerViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterOrganizerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _api.RegisterOrganizerAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Login));
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The API could not be reached. Make sure KMCEventAPI is running.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "You have been logged out.";
        return RedirectToAction("Index", "Home");
    }
}
