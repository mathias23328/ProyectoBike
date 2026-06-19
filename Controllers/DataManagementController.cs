using Microsoft.AspNetCore.Mvc;
using grupomathias.Services;
using System.ComponentModel.DataAnnotations;

namespace grupomathias.Controllers;

/// <summary>
/// Controlador que demuestra todos los patrones de almacenamiento de datos en ASP.NET Core:
/// - Sesiones (Session)
/// - Cookies (Request/Response Cookies)
/// - ViewData (datos de la vista actual)
/// - TempData (datos temporales entre redirecciones)
/// - Redis Cache (caché distribuida)
/// - HTTP Methods (GET, POST, PUT, DELETE)
/// </summary>
public class DataManagementController : Controller
{
    private readonly IWebDataService _webDataService;
    private readonly ILogger<DataManagementController> _logger;

    public DataManagementController(
        IWebDataService webDataService,
        ILogger<DataManagementController> logger)
    {
        _webDataService = webDataService;
        _logger = logger;
    }

    #region Vistas
    /// <summary>
    /// GET: /DataManagement
    /// Página principal del demo de almacenamiento de datos
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Gestión de Datos";
        ViewData["Description"] = "Demuestra Sessions, Cookies, ViewData, TempData y Redis Cache";

        var model = new Dictionary<string, object?>
        {
            { "CurrentTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "SessionExample", _webDataService.GetSessionValue("demo_session") ?? "Sin sesión" },
            { "CookieExample", _webDataService.GetCookie("demo_cookie") ?? "Sin cookie" },
            { "CacheExample", await _webDataService.GetCacheAsync<string>("demo_cache") ?? "Sin caché" }
        };

        return View(model);
    }

    #endregion

    #region SESIONES - HTTP GET/POST
    /// <summary>
    /// GET: /DataManagement/Sessions
    /// Página para interactuar con sesiones
    /// </summary>
    [HttpGet]
    public IActionResult Sessions()
    {
        ViewData["Title"] = "Gestión de Sesiones";
        var sessionData = _webDataService.GetSessionValue("UserSession");
        ViewData["SessionData"] = sessionData ?? "No hay sesión activa";
        return View();
    }

    /// <summary>
    /// POST: /DataManagement/Sessions
    /// Guarda datos en la sesión del usuario
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Sessions([Required] string sessionValue)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("El valor de sesión es requerido");
        }

        _webDataService.SetSessionValue("UserSession", sessionValue);
        TempData["Success"] = $"Sesión guardada: {sessionValue}";
        
        return RedirectToAction(nameof(Sessions));
    }

    /// <summary>
    /// POST: /DataManagement/ClearSession
    /// Limpia toda la sesión del usuario
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ClearSession()
    {
        _webDataService.ClearSession();
        TempData["Success"] = "Sesión limpiada correctamente";
        return RedirectToAction(nameof(Sessions));
    }
    #endregion

    #region COOKIES - HTTP GET/POST/DELETE
    /// <summary>
    /// GET: /DataManagement/Cookies
    /// Página para interactuar con cookies
    /// </summary>
    [HttpGet]
    public IActionResult Cookies()
    {
        ViewData["Title"] = "Gestión de Cookies";
        var cookieData = _webDataService.GetCookie("UserCookie");
        ViewData["CookieData"] = cookieData ?? "No hay cookie activa";
        return View();
    }

    /// <summary>
    /// POST: /DataManagement/Cookies
    /// Guarda una cookie en el navegador
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cookies([Required] string cookieValue, [Range(1, 365)] int daysExpiration = 30)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("El valor de cookie y duración son requeridos");
        }

        _webDataService.SetCookie("UserCookie", cookieValue, daysExpiration);
        TempData["Success"] = $"Cookie guardada por {daysExpiration} días: {cookieValue}";
        
        return RedirectToAction(nameof(Cookies));
    }

    /// <summary>
    /// POST: /DataManagement/Cookies/Secure
    /// Guarda una cookie segura (HttpOnly + Secure)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetSecureCookie([Required] string cookieValue, [Range(1, 365)] int daysExpiration = 30)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("El valor de cookie es requerido");
        }

        _webDataService.SetSecureCookie("SecureUserCookie", cookieValue, daysExpiration);
        TempData["Success"] = $"Cookie segura guardada: {cookieValue}";
        
        return RedirectToAction(nameof(Cookies));
    }

    /// <summary>
    /// DELETE: /DataManagement/Cookies
    /// Elimina una cookie
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteCookie(string cookieName = "UserCookie")
    {
        _webDataService.RemoveCookie(cookieName);
        TempData["Success"] = $"Cookie eliminada: {cookieName}";
        
        return RedirectToAction(nameof(Cookies));
    }
    #endregion

    #region VIEWDATA - GET
    /// <summary>
    /// GET: /DataManagement/ViewDataDemo
    /// Demuestra el uso de ViewData (solo persiste en la vista actual)
    /// </summary>
    [HttpGet]
    public IActionResult ViewDataDemo()
    {
        ViewData["Title"] = "ViewData Demo";
        ViewData["Message"] = "Este es un mensaje en ViewData";
        ViewData["Timestamp"] = DateTime.Now;
        ViewData["UserAgent"] = Request.Headers["User-Agent"].ToString();
        ViewData["IpAddress"] = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        
        return View();
    }
    #endregion

    #region TEMPDATA - POST/Redirect
    /// <summary>
    /// GET: /DataManagement/TempData
    /// Página para interactuar con TempData
    /// </summary>
    [HttpGet]
    public IActionResult TempDataDemo()
    {
        ViewData["Title"] = "TempData Demo";
        return View();
    }

    /// <summary>
    /// POST: /DataManagement/TempData
    /// Guarda datos en TempData y redirige
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetTempData([Required] string tempValue)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("El valor de TempData es requerido");
        }

        TempData["UserMessage"] = tempValue;
        TempData["Timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        return RedirectToAction(nameof(TempDataResult));
    }

    /// <summary>
    /// GET: /DataManagement/TempDataResult
    /// Muestra los datos guardados en TempData
    /// </summary>
    [HttpGet]
    public IActionResult TempDataResult()
    {
        ViewData["Title"] = "TempData Resultado";
        // TempData está disponible aquí después de la redirección
        // Una vez leído, se elimina automáticamente
        return View();
    }
    #endregion

    #region REDIS CACHE - GET/POST/PUT/DELETE
    /// <summary>
    /// GET: /DataManagement/Cache
    /// Página para interactuar con Redis Cache
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Cache()
    {
        ViewData["Title"] = "Redis Cache Demo";
        
        var cacheData = await _webDataService.GetCacheAsync<string>("DemoCache");
        ViewData["CacheData"] = cacheData ?? "No hay datos en caché";
        
        var keyExists = await _webDataService.KeyExistsCacheAsync("DemoCache");
        ViewData["KeyExists"] = keyExists;
        
        return View();
    }

    /// <summary>
    /// POST: /DataManagement/Cache
    /// Guarda datos en Redis Cache
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cache([Required] string cacheValue, [Range(1, 1440)] int minutesExpiration = 30)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("El valor de caché es requerido");
        }

        await _webDataService.SetCacheAsync("DemoCache", cacheValue, TimeSpan.FromMinutes(minutesExpiration));
        TempData["Success"] = $"Caché guardado por {minutesExpiration} minutos: {cacheValue}";
        
        return RedirectToAction(nameof(Cache));
    }

    /// <summary>
    /// PUT: /DataManagement/Cache/Update
    /// Actualiza un valor en el caché (HTTP PUT)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCache([Required] string cacheValue)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("El valor de caché es requerido");
        }

        await _webDataService.RemoveCacheAsync("DemoCache");
        await _webDataService.SetCacheAsync("DemoCache", cacheValue, TimeSpan.FromMinutes(60));
        TempData["Success"] = $"Caché actualizado: {cacheValue}";
        
        return RedirectToAction(nameof(Cache));
    }

    /// <summary>
    /// DELETE: /DataManagement/Cache
    /// Elimina un valor del caché
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCache()
    {
        await _webDataService.RemoveCacheAsync("DemoCache");
        TempData["Success"] = "Caché eliminado correctamente";
        
        return RedirectToAction(nameof(Cache));
    }

    /// <summary>
    /// POST: /DataManagement/CacheData
    /// Guarda datos estructurados en caché
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CacheComplexData(
        [Required] string title,
        [Required] string description,
        [Range(1, 100)] int priority = 5)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Todos los campos son requeridos");
        }

        var data = new
        {
            Title = title,
            Description = description,
            Priority = priority,
            CreatedAt = DateTime.Now
        };

        await _webDataService.SetCacheAsync("ComplexData", data, TimeSpan.FromMinutes(60));
        TempData["Success"] = "Datos complejos guardados en caché";
        
        return RedirectToAction(nameof(Cache));
    }
    #endregion

    #region FORMULARIOS - GET/POST con validación
    /// <summary>
    /// GET: /DataManagement/Forms
    /// Página de formularios con demostración de validación
    /// </summary>
    [HttpGet]
    public IActionResult Forms()
    {
        ViewData["Title"] = "Formularios y Validación";
        return View();
    }

    /// <summary>
    /// POST: /DataManagement/Forms
    /// Procesa un formulario con validación
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Forms(FormDataModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Guardar en diferentes almacenes
        _webDataService.SetSessionValue("FormName", model.Name);
        _webDataService.SetCookie("FormEmail", model.Email);
        TempData["FormSuccess"] = $"Formulario procesado: {model.Name}";

        // Guardar en caché también
        var formData = new { model.Name, model.Email, model.Message, Timestamp = DateTime.Now };
        _ = _webDataService.SetCacheAsync("LastFormData", formData, TimeSpan.FromHours(1));

        return RedirectToAction(nameof(FormResult));
    }

    /// <summary>
    /// GET: /DataManagement/FormResult
    /// Muestra el resultado del procesamiento del formulario
    /// </summary>
    [HttpGet]
    public IActionResult FormResult()
    {
        ViewData["Title"] = "Resultado del Formulario";
        var formName = _webDataService.GetSessionValue("FormName");
        ViewData["FormName"] = formName ?? "Sin nombre";
        return View();
    }
    #endregion

    #region MÉTODOS HTTP - Ejemplos completos
    /// <summary>
    /// GET: /DataManagement/HttpMethods
    /// Demuestra todos los métodos HTTP
    /// </summary>
    [HttpGet]
    public IActionResult HttpMethods()
    {
        ViewData["Title"] = "Métodos HTTP";
        ViewData["Methods"] = new[]
        {
            new { Method = "GET", Description = "Obtener datos", Example = "/DataManagement/HttpMethods" },
            new { Method = "POST", Description = "Crear/Enviar datos", Example = "/DataManagement/HttpMethods (form submit)" },
            new { Method = "PUT", Description = "Actualizar recursos", Example = "/api/data/1 (JSON body)" },
            new { Method = "DELETE", Description = "Eliminar recursos", Example = "/api/data/1" }
        };
        return View();
    }

    /// <summary>
    /// POST: /DataManagement/HttpMethods
    /// Ejemplo de POST - Crear
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult HttpMethods([Required] string data)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("Datos requeridos");
        }

        TempData["Message"] = $"POST recibido: {data}";
        return RedirectToAction(nameof(HttpMethods));
    }
    #endregion

    #region MAPAS - Rutas de bicicleta en Lima
    /// <summary>
    /// GET: /DataManagement/Maps
    /// Mapa interactivo de rutas de bicicleta en Lima
    /// </summary>
    [HttpGet]
    public IActionResult Maps()
    {
        ViewData["Title"] = "Mapa de Rutas - Lima";
        
        // Datos de rutas populares en Lima con coordenadas
        var routes = new List<object>
        {
            new { Name = "Miraflores - Costa Verde", Lat = -12.1282, Lng = -77.2275, Distance = 12.5, Difficulty = "Fácil" },
            new { Name = "San Isidro - Parque El Olivar", Lat = -12.1208, Lng = -77.0371, Distance = 8.0, Difficulty = "Fácil" },
            new { Name = "Ciclovía Metropolitana", Lat = -12.0931, Lng = -77.0424, Distance = 25.0, Difficulty = "Media" },
            new { Name = "Barranco - Bohemia", Lat = -12.1503, Lng = -77.2406, Distance = 5.5, Difficulty = "Fácil" },
            new { Name = "Surco - Chaclacayo", Lat = -12.1628, Lng = -76.8956, Distance = 35.0, Difficulty = "Difícil" },
            new { Name = "Breña - Mamacona", Lat = -12.0558, Lng = -76.8628, Distance = 40.0, Difficulty = "Difícil" }
        };

        ViewData["Routes"] = routes;
        ViewData["LimaCenter"] = new { Lat = -12.0931, Lng = -77.0424 };
        
        return View();
    }

    /// <summary>
    /// POST: /DataManagement/Maps
    /// Guarda una ruta favorita
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Maps([Required] string routeName)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest("El nombre de la ruta es requerido");
        }

        _webDataService.SetCookie("FavoriteRoute", routeName, 365);
        _webDataService.SetSessionValue("LastRoute", routeName);
        TempData["Success"] = $"Ruta favorita guardada: {routeName}";

        return RedirectToAction(nameof(Maps));
    }

    /// <summary>
    /// GET: /DataManagement/Maps/Distance
    /// Calcula la distancia entre dos puntos
    /// </summary>
    [HttpGet]
    public IActionResult CalculateDistance(double lat1, double lng1, double lat2, double lng2)
    {
        // Fórmula de Haversine para calcular distancia entre dos coordenadas
        const double R = 6371; // Radio de la Tierra en km
        
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLng = (lng2 - lng1) * Math.PI / 180;
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distance = R * c;

        return Json(new { Distance = distance.ToString("F2"), Unit = "km" });
    }
    #endregion
}

/// <summary>
/// Modelo para formularios con validación
/// </summary>
public class FormDataModel
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    public string Email { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "El mensaje no puede exceder 500 caracteres")]
    public string Message { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "La calificación debe estar entre 1 y 10")]
    public int? Rating { get; set; }
}
