using mathiasBike.Models;
using Microsoft.AspNetCore.Mvc;

namespace mathiasBike.Controllers;

public class HttpDemoController : Controller
{
    [HttpGet]
    public IActionResult Index(string? buscar)
    {
        var model = new HttpDemoViewModel
        {
            Metodo = HttpContext.Request.Method,
            Ruta = HttpContext.Request.Path,
            Consulta = HttpContext.Request.QueryString.HasValue
                ? HttpContext.Request.QueryString.Value!
                : "(sin parámetros)",
            ValorEnviado = buscar,
            Explicacion = "GET envía datos en la URL (query string). Es idempotente: repetir la petición no modifica el servidor."
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(HttpDemoViewModel input)
    {
        var model = new HttpDemoViewModel
        {
            Metodo = HttpContext.Request.Method,
            Ruta = HttpContext.Request.Path,
            Consulta = HttpContext.Request.QueryString.HasValue
                ? HttpContext.Request.QueryString.Value!
                : "(sin parámetros)",
            ValorEnviado = input.ValorEnviado,
            Explicacion = "POST envía datos en el cuerpo del formulario. Se usa para crear o modificar recursos."
        };

        return View(model);
    }
}
