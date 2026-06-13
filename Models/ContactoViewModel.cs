using System.ComponentModel.DataAnnotations;

namespace mathiasBike.Models;

public class ContactoViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2)]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Introduce un correo válido")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El asunto es obligatorio")]
    [StringLength(150)]
    [Display(Name = "Asunto")]
    public string Asunto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El mensaje es obligatorio")]
    [StringLength(1000, MinimumLength = 10)]
    [Display(Name = "Mensaje")]
    [DataType(DataType.MultilineText)]
    public string Mensaje { get; set; } = string.Empty;

    [Display(Name = "Acepto la política de privacidad")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Debes aceptar la política de privacidad")]
    public bool AceptaPolitica { get; set; }
}
