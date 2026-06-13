using System.ComponentModel.DataAnnotations;

namespace grupomathias.Models;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required]
    [Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;
    
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class PlatformDemoViewModel
{
    public string WelcomeMessage { get; set; } = string.Empty;

    public string SessionVisitMessage { get; set; } = string.Empty;

    public string TempDataMessage { get; set; } = string.Empty;

    public string CookieMessage { get; set; } = string.Empty;

    public string CacheMessage { get; set; } = string.Empty;

    public string CacheProvider { get; set; } = string.Empty;

    public string DatabaseSummary { get; set; } = string.Empty;

    public string IdentityStatus { get; set; } = string.Empty;

    public List<BikeRouteSummary> RecentRoutes { get; set; } = [];
}

public class BikeRouteSummary
{
    public string Origin { get; set; } = string.Empty;

    public string Destination { get; set; } = string.Empty;

    public int DistanceKm { get; set; }

    public int SafetyScore { get; set; }
}