using mathiasBike.Models;
using Microsoft.AspNetCore.Mvc;

namespace mathiasBike.Controllers;

public class ReporteController : Controller
{
    [HttpGet]
    public IActionResult Crear()
    {
        return View(new ReporteRutaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(ReporteRutaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ViewData["Titulo"] = "Reporte enviado";
        ViewData["Mensaje"] = $"Gracias. Tu reporte sobre \"{model.NombreRuta}\" ({model.NivelRiesgo}) fue recibido.";
        return View("Confirmacion");
    }
}
