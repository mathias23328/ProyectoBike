using System.ComponentModel.DataAnnotations;

namespace mathiasBike.Models;

public class ReporteRutaViewModel
{
    [Required(ErrorMessage = "Indica el nombre de la ruta")]
    [Display(Name = "Nombre de la ruta")]
    public string NombreRuta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona el nivel de riesgo")]
    [Display(Name = "Nivel de riesgo")]
    public string NivelRiesgo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Describe el incidente")]
    [StringLength(500, MinimumLength = 10)]
    [Display(Name = "Descripción del reporte")]
    [DataType(DataType.MultilineText)]
    public string Descripcion { get; set; } = string.Empty;

    [Display(Name = "Incluir ubicación aproximada")]
    public bool IncluirUbicacion { get; set; }
}
