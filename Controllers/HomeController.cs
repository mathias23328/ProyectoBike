using System.Diagnostics;
using grupomathias.Data;
using grupomathias.Models;
using grupomathias.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace grupomathias.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly IWebHostEnvironment _environment;
    private readonly IBikeRouteAgentService _bikeRouteAgentService;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext dbContext,
        IDistributedCache cache,
        IWebHostEnvironment environment,
        IBikeRouteAgentService bikeRouteAgentService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _cache = cache;
        _environment = environment;
        _bikeRouteAgentService = bikeRouteAgentService;
    }

    public IActionResult Index()
    {
        return View(CreateDefaultModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RoutePlannerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ApplyRouteRecommendation(model);
        await SaveRouteLogAsync(model);
        await UpdatePlatformStateAsync(model);
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Ml()
    {
        var status = await _bikeRouteAgentService.GetStatusAsync();
        return View(new MlAnalysisViewModel
        {
            Request = new RouteAnalysisRequest
            {
                Origin = "Centro histórico",
                Destination = "Parque lineal",
                DistanceKm = 8,
                RiderType = "urbano",
                DepartureWindow = "Tarde",
                PreferBikeLanes = true,
                AvoidHighTraffic = true,
                PreferWellLitAreas = true
            },
            Status = status
        });
    }

    [HttpGet]
    public IActionResult Chat()
    {
        ViewData["Title"] = "Chatbot";
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ml(MlAnalysisViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Status = await _bikeRouteAgentService.GetStatusAsync();
            return View(model);
        }

        model.Response = await _bikeRouteAgentService.AnalyzeAsync(model.Request);
        model.Status = await _bikeRouteAgentService.GetStatusAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Platform()
    {
        ViewData["Title"] = "Plataforma web";
        return View(await BuildPlatformDemoModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SessionAction(string visitorName)
    {
        HttpContext.Session.SetString("VisitorName", visitorName);
        TempData["PlatformNotice"] = $"Sesión guardada para {visitorName}.";
        return RedirectToAction(nameof(Platform));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CookieAction(string preferredTheme)
    {
        Response.Cookies.Append("bike-theme", preferredTheme, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(15),
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        });

        TempData["PlatformNotice"] = $"Cookie guardada: {preferredTheme}.";
        return RedirectToAction(nameof(Platform));
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

    private PlatformDemoViewModel BuildPlatformDemoModel()
    {
        var visitorName = HttpContext.Session.GetString("VisitorName") ?? "visitante";
        var cookieTheme = Request.Cookies["bike-theme"] ?? "modo seguro";
        var cacheProvider = _environment.EnvironmentName == Environments.Development ? "Memory cache local" : "Redis si está configurado";
        var lastRoute = _cache.GetString("bike:last-route") ?? "Sin ruta en caché todavía.";

        return new PlatformDemoViewModel
        {
            WelcomeMessage = User.Identity?.IsAuthenticated == true
                ? $"Usuario autenticado: {User.Identity.Name}"
                : "Usuario no autenticado todavía.",
            SessionVisitMessage = $"Sesión activa para {visitorName}.",
            TempDataMessage = TempData["PlatformNotice"]?.ToString() ?? "Sin mensaje de TempData en esta carga.",
            CookieMessage = $"Cookie leída: {cookieTheme}.",
            CacheProvider = cacheProvider,
            CacheMessage = $"Cache de ruta: {lastRoute}",
            DatabaseSummary = _dbContext.BikeRouteLogs.Any()
                ? $"Rutas guardadas: {_dbContext.BikeRouteLogs.Count()}"
                : "Aún no hay rutas guardadas en la base de datos SQLite.",
            IdentityStatus = User.Identity?.IsAuthenticated == true ? "Sesión iniciada con Identity." : "Lista para registrar e iniciar sesión.",
            RecentRoutes = _dbContext.BikeRouteLogs
                .OrderByDescending(route => route.CreatedAtUtc)
                .Take(5)
                .Select(route => new BikeRouteSummary
                {
                    Origin = route.Origin,
                    Destination = route.Destination,
                    DistanceKm = route.DistanceKm,
                    SafetyScore = route.SafetyScore
                })
                .ToList()
        };
    }

    private async Task<PlatformDemoViewModel> BuildPlatformDemoModelAsync()
    {
        var model = BuildPlatformDemoModel();
        model.RecentRoutes = await _dbContext.BikeRouteLogs
            .OrderByDescending(route => route.CreatedAtUtc)
            .Take(5)
            .Select(route => new BikeRouteSummary
            {
                Origin = route.Origin,
                Destination = route.Destination,
                DistanceKm = route.DistanceKm,
                SafetyScore = route.SafetyScore
            })
            .ToListAsync();

        return model;
    }

    private async Task SaveRouteLogAsync(RoutePlannerViewModel model)
    {
        _dbContext.BikeRouteLogs.Add(new BikeRouteLog
        {
            Origin = model.Origin,
            Destination = model.Destination,
            DistanceKm = model.DistanceKm,
            RiderType = model.RiderType,
            SafetyScore = model.SafetyScore,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }

    private async Task UpdatePlatformStateAsync(RoutePlannerViewModel model)
    {
        HttpContext.Session.SetString("LastRoute", $"{model.Origin} -> {model.Destination}");
        Response.Cookies.Append("bike-last-score", model.SafetyScore.ToString(), new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddDays(7),
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        });

        TempData["PlatformNotice"] = $"Ruta guardada con {model.SafetyScore}% de seguridad.";

        await _cache.SetStringAsync(
            "bike:last-route",
            $"{model.Origin} => {model.Destination} ({model.SafetyScore}%)",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
            });
    }
}
