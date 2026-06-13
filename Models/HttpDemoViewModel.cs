namespace mathiasBike.Models;

public class HttpDemoViewModel
{
    public string Metodo { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
    public string Consulta { get; set; } = string.Empty;
    public string? ValorEnviado { get; set; }
    public string Explicacion { get; set; } = string.Empty;
}
