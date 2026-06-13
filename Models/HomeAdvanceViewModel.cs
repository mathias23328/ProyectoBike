using System.ComponentModel.DataAnnotations;

namespace mathiasBike.Models;

public class HomeAdvanceViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2)]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Introduce un correo válido")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona una práctica")]
    [Display(Name = "Práctica")]
    public string PracticaSeleccionada { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tema principal es obligatorio")]
    [StringLength(120, MinimumLength = 3)]
    [Display(Name = "Tema principal")]
    public string TemaPrincipal { get; set; } = string.Empty;

    [Required(ErrorMessage = "Agrega un comentario")]
    [StringLength(1000, MinimumLength = 10)]
    [Display(Name = "Comentario del avance")]
    [DataType(DataType.MultilineText)]
    public string Comentario { get; set; } = string.Empty;

    [Display(Name = "Acepto que mi avance quede registrado")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Debes aceptar antes de enviar")]
    public bool AceptaTerminos { get; set; }

    public int VisitCount { get; set; }

    public string SavedPreference { get; set; } = string.Empty;

    public string LastVisit { get; set; } = string.Empty;
}