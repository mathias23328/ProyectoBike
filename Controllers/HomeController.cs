using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using mathiasBike.Models;

namespace mathiasBike.Controllers;

public class HomeController : Controller
{
    private const string VisitCountKey = "HomeVisitCount";
    private const string PreferenceCookieKey = "bike_preference";

    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var visitCount = (HttpContext.Session.GetInt32(VisitCountKey) ?? 0) + 1;
        HttpContext.Session.SetInt32(VisitCountKey, visitCount);

        var savedPreference = Request.Cookies[PreferenceCookieKey];
        var model = new HomeAdvanceViewModel
        {
            VisitCount = visitCount,
            SavedPreference = string.IsNullOrWhiteSpace(savedPreference)
                ? "Sin preferencia guardada"
                : savedPreference,
            LastVisit = DateTimeOffset.Now.ToString("g", CultureInfo.CurrentCulture)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(HomeAdvanceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        TempData["AdvanceMessage"] = $"Gracias, {model.Nombre}. Recibimos tu avance de {model.PracticaSeleccionada}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GuardarPreferencia(string rutaPreferida)
    {
        if (string.IsNullOrWhiteSpace(rutaPreferida))
        {
            TempData["AdvanceMessage"] = "Selecciona una preferencia antes de guardar.";
            return RedirectToAction(nameof(Index));
        }

        Response.Cookies.Append(
            PreferenceCookieKey,
            rutaPreferida,
            new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                IsEssential = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            });

        TempData["AdvanceMessage"] = $"Preferencia guardada: {rutaPreferida}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReiniciarEstado()
    {
        HttpContext.Session.Remove(VisitCountKey);
        Response.Cookies.Delete(PreferenceCookieKey);
        TempData["AdvanceMessage"] = "Sesión y cookie reiniciadas.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
