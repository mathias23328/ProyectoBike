using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using grupomathias.Models;

namespace grupomathias.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(CreateDefaultModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(RoutePlannerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ApplyRouteRecommendation(model);
        return View(model);
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

    private static RoutePlannerViewModel CreateDefaultModel()
    {
        var model = new RoutePlannerViewModel
        {
            Origin = "Centro histórico",
            Destination = "Parque lineal",
            DistanceKm = 8,
            RiderType = "urbano",
            DepartureWindow = "Tarde",
            PreferBikeLanes = true,
            AvoidHighTraffic = true,
            PreferWellLitAreas = true
        };

        ApplyRouteRecommendation(model);
        return model;
    }

    private static void ApplyRouteRecommendation(RoutePlannerViewModel model)
    {
        var safetyScore = 45;

        if (model.PreferBikeLanes)
        {
            safetyScore += 20;
        }

        if (model.AvoidHighTraffic)
        {
            safetyScore += 15;
        }

        if (model.PreferWellLitAreas)
        {
            safetyScore += model.DepartureWindow == "Noche" ? 15 : 8;
        }

        if (model.DistanceKm <= 10)
        {
            safetyScore += 10;
        }

        if (model.RiderType == "familiar")
        {
            safetyScore += 5;
        }

        model.SafetyScore = Math.Clamp(safetyScore, 0, 100);

        if (model.PreferBikeLanes && model.AvoidHighTraffic)
        {
            model.RecommendedRouteTitle = "Corredor ciclista protegido";
            model.RecommendedRouteSummary = "Prioriza ciclovías, cruces tranquilos y tramos con menor exposición vehicular.";
        }
        else if (model.DistanceKm > 12)
        {
            model.RecommendedRouteTitle = "Ruta mixta por parques y vías calmadas";
            model.RecommendedRouteSummary = "Combina avenidas secundarias con zonas verdes para mantener ritmo y visibilidad.";
        }
        else if (model.RiderType == "familiar")
        {
            model.RecommendedRouteTitle = "Ruta barrial tranquila";
            model.RecommendedRouteSummary = "Diseñada para trayectos relajados, con menos pendientes y mejor control de cruces.";
        }
        else
        {
            model.RecommendedRouteTitle = "Ruta eficiente con cruces seguros";
            model.RecommendedRouteSummary = "Reduce el tiempo de viaje manteniendo puntos de cruce amplios y bien señalizados.";
        }

        model.RouteHighlights = new List<string>
        {
            $"Salida sugerida desde {model.Origin} hacia {model.Destination}.",
            model.PreferBikeLanes ? "Incluye tramos con infraestructura ciclista priorizada." : "Evita pasos complejos y avenidas con tráfico intenso.",
            model.PreferWellLitAreas ? "Favorece calles con iluminación y mayor visibilidad." : "Mantiene una ruta corta y directa para reducir exposición."
        };

        model.RouteAdvice = model.SafetyScore >= 80
            ? "Ruta con nivel alto de seguridad para desplazarte con confianza."
            : model.SafetyScore >= 60
                ? "Ruta equilibrada: segura y práctica para uso urbano diario."
                : "Conviene revisar el trayecto final en el mapa antes de salir.";
    }
}
