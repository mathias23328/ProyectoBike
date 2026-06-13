using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using mathiasBike.Models;

namespace mathiasBike.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new HomeAdvanceViewModel());
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
