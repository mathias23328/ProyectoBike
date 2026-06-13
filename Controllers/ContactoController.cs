using mathiasBike.Models;
using Microsoft.AspNetCore.Mvc;

namespace mathiasBike.Controllers;

public class ContactoController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new ContactoViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(ContactoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction(nameof(Gracias), new { nombre = model.Nombre, asunto = model.Asunto });
    }

    [HttpGet]
    public IActionResult Gracias(string nombre, string asunto)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return RedirectToAction(nameof(Index));
        }

        ViewData["Nombre"] = nombre;
        ViewData["Asunto"] = asunto;
        return View();
    }
}
