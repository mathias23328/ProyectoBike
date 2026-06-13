using System.ComponentModel.DataAnnotations;

namespace grupomathias.Models;

public class RoutePlannerViewModel
{
    [Required(ErrorMessage = "Ingresa un punto de salida.")]
    [Display(Name = "Origen")]
    public string Origin { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa un destino.")]
    [Display(Name = "Destino")]
    public string Destination { get; set; } = string.Empty;

    [Range(1, 60, ErrorMessage = "La distancia debe estar entre 1 y 60 km.")]
    [Display(Name = "Distancia estimada (km)")]
    public int DistanceKm { get; set; } = 8;

    [Required(ErrorMessage = "Selecciona el tipo de recorrido.")]
    [Display(Name = "Tipo de recorrido")]
    public string RiderType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona una franja horaria.")]
    [Display(Name = "Franja de salida")]
    public string DepartureWindow { get; set; } = string.Empty;

    [Display(Name = "Priorizar ciclovías")]
    public bool PreferBikeLanes { get; set; }

    [Display(Name = "Evitar avenidas con tráfico pesado")]
    public bool AvoidHighTraffic { get; set; }

    [Display(Name = "Priorizar zonas iluminadas")]
    public bool PreferWellLitAreas { get; set; }

    public string RecommendedRouteTitle { get; set; } = string.Empty;

    public string RecommendedRouteSummary { get; set; } = string.Empty;

    public int SafetyScore { get; set; }

    public string RouteAdvice { get; set; } = string.Empty;

    public List<string> RouteHighlights { get; set; } = [];
}