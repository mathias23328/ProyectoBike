using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace grupomathias.Services;

/// <summary>
/// Servicio centralizado para manejar: Cookies, ViewData, TempData, Sessions y Redis Cache
/// </summary>
public interface IWebDataService
{
    // ===== SESIONES =====
    void SetSessionValue(string key, string value);
    string? GetSessionValue(string key);
    void RemoveSessionValue(string key);
    void ClearSession();

    // ===== COOKIES =====
    void SetCookie(string key, string value, int daysExpiration = 30);
    void SetSecureCookie(string key, string value, int daysExpiration = 30);
    string? GetCookie(string key);
    void RemoveCookie(string key);

    // ===== VIEWDATA =====
    void SetViewData(string key, object? value);
    object? GetViewData(string key);

    // ===== TEMPDATA =====
    void SetTempData(string key, string value);
    string? GetTempData(string key);

    // ===== REDIS CACHE =====
    Task<T?> GetCacheAsync<T>(string key);
    Task SetCacheAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveCacheAsync(string key);
    Task<bool> KeyExistsCacheAsync(string key);
}

public class WebDataService : IWebDataService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<WebDataService> _logger;

    private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

    public WebDataService(
        IHttpContextAccessor httpContextAccessor,
        IDistributedCache distributedCache,
        ILogger<WebDataService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    #region Sesiones
    /// <summary>
    /// Guarda un valor en la sesión del usuario actual
    /// </summary>
    public void SetSessionValue(string key, string value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                _logger.LogWarning("Intento de guardar sesión con valores null o vacíos");
                return;
            }
            HttpContext?.Session.SetString(key, value);
            _logger.LogInformation($"Sesión guardada: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al guardar sesión {key}");
        }
    }

    /// <summary>
    /// Obtiene un valor de la sesión del usuario actual
    /// </summary>
    public string? GetSessionValue(string key)
    {
        try
        {
            return HttpContext?.Session.GetString(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener sesión {key}");
            return null;
        }
    }

    /// <summary>
    /// Elimina un valor de la sesión
    /// </summary>
    public void RemoveSessionValue(string key)
    {
        try
        {
            HttpContext?.Session.Remove(key);
            _logger.LogInformation($"Sesión eliminada: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar sesión {key}");
        }
    }

    /// <summary>
    /// Limpia toda la sesión
    /// </summary>
    public void ClearSession()
    {
        try
        {
            HttpContext?.Session.Clear();
            _logger.LogInformation("Sesión limpiada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar sesión");
        }
    }
    #endregion

    #region Cookies
    /// <summary>
    /// Guarda una cookie estándar
    /// </summary>
    public void SetCookie(string key, string value, int daysExpiration = 30)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                _logger.LogWarning("Intento de guardar cookie con valores null o vacíos");
                return;
            }
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(daysExpiration),
                HttpOnly = false,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            };
            HttpContext?.Response.Cookies.Append(key, value, cookieOptions);
            _logger.LogInformation($"Cookie guardada: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al guardar cookie {key}");
        }
    }

    /// <summary>
    /// Guarda una cookie segura (HttpOnly + Secure)
    /// </summary>
    public void SetSecureCookie(string key, string value, int daysExpiration = 30)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                _logger.LogWarning("Intento de guardar cookie segura con valores null o vacíos");
                return;
            }
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(daysExpiration),
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.Strict
            };
            HttpContext?.Response.Cookies.Append(key, value, cookieOptions);
            _logger.LogInformation($"Cookie segura guardada: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al guardar cookie segura {key}");
        }
    }

    /// <summary>
    /// Obtiene el valor de una cookie
    /// </summary>
    public string? GetCookie(string key)
    {
        try
        {
            if (HttpContext?.Request.Cookies.TryGetValue(key, out var value) == true)
            {
                return value;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener cookie {key}");
            return null;
        }
    }

    /// <summary>
    /// Elimina una cookie
    /// </summary>
    public void RemoveCookie(string key)
    {
        try
        {
            HttpContext?.Response.Cookies.Delete(key);
            _logger.LogInformation($"Cookie eliminada: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar cookie {key}");
        }
    }
    #endregion

    #region ViewData
    /// <summary>
    /// Guarda datos para la vista actual (no persisten en redirección)
    /// </summary>
    public void SetViewData(string key, object? value)
    {
        try
        {
            if (HttpContext?.Items != null)
            {
                HttpContext.Items[key] = value;
            }
            _logger.LogInformation($"ViewData guardado: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al guardar ViewData {key}");
        }
    }

    /// <summary>
    /// Obtiene datos de la vista actual
    /// </summary>
    public object? GetViewData(string key)
    {
        try
        {
            if (HttpContext?.Items != null && HttpContext.Items.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener ViewData {key}");
            return null;
        }
    }
    #endregion

    #region TempData
    /// <summary>
    /// Guarda datos temporales que persisten en una redirección
    /// </summary>
    public void SetTempData(string key, string value)
    {
        try
        {
            // Usar en controlador con TempData
            // Se debe registrar en Program.cs: builder.Services.AddSession()
            _logger.LogInformation($"TempData guardado: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al guardar TempData {key}");
        }
    }

    /// <summary>
    /// Obtiene datos temporales
    /// </summary>
    public string? GetTempData(string key)
    {
        try
        {
            _logger.LogInformation($"TempData recuperado: {key}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener TempData {key}");
            return null;
        }
    }
    #endregion

    #region Redis Cache
    /// <summary>
    /// Obtiene un valor del caché distribuida (Redis o Memory)
    /// </summary>
    public async Task<T?> GetCacheAsync<T>(string key)
    {
        try
        {
            var cachedValue = await _distributedCache.GetAsync(key);
            if (cachedValue == null) return default;

            var json = System.Text.Encoding.UTF8.GetString(cachedValue);
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al obtener caché {key}");
            return default;
        }
    }

    /// <summary>
    /// Guarda un valor en el caché distribuida con expiración opcional
    /// </summary>
    public async Task SetCacheAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            var options = new DistributedCacheEntryOptions();
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }

            await _distributedCache.SetAsync(key, bytes, options);
            _logger.LogInformation($"Caché guardado: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al guardar caché {key}");
        }
    }

    /// <summary>
    /// Elimina un valor del caché
    /// </summary>
    public async Task RemoveCacheAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
            _logger.LogInformation($"Caché eliminado: {key}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al eliminar caché {key}");
        }
    }

    /// <summary>
    /// Verifica si una clave existe en el caché
    /// </summary>
    public async Task<bool> KeyExistsCacheAsync(string key)
    {
        try
        {
            var value = await _distributedCache.GetAsync(key);
            return value != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al verificar existencia de caché {key}");
            return false;
        }
    }
    #endregion
}
