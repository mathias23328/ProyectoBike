using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace grupomathias.Models;

public class MembershipPageViewModel
{
    public string PlanName { get; set; } = "Bike Plus";

    public decimal MonthlyPrice { get; set; } = 19.90m;

    public string Currency { get; set; } = "PEN";

    public string BenefitsSummary { get; set; } = string.Empty;

    public string ApiHint { get; set; } = string.Empty;

    // Mercado Pago public key for client-side SDK
    public string PublicKey { get; set; } = string.Empty;
}

public class MembershipCheckoutRequest
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PlanName { get; set; } = string.Empty;

    [Range(1, 9999)]
    public decimal MonthlyPrice { get; set; }
}

public class MembershipCheckoutResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? PreferenceId { get; set; }

    public string? CheckoutUrl { get; set; }

    public string? SandboxUrl { get; set; }
}

public class MercadoPagoPreferencePayload
{
    [JsonPropertyName("items")]
    public MercadoPagoPreferenceItem[] Items { get; set; } = [];

    [JsonPropertyName("payer")]
    public MercadoPagoPayer? Payer { get; set; }

    [JsonPropertyName("auto_return")]
    public string AutoReturn { get; set; } = "approved";

    [JsonPropertyName("back_urls")]
    public MercadoPagoBackUrls BackUrls { get; set; } = new();

    [JsonPropertyName("external_reference")]
    public string ExternalReference { get; set; } = string.Empty;
}

public class MercadoPagoPreferenceItem
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;

    [JsonPropertyName("unit_price")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("currency_id")]
    public string CurrencyId { get; set; } = "PEN";
}

public class MercadoPagoPayer
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class MercadoPagoBackUrls
{
    [JsonPropertyName("success")]
    public string Success { get; set; } = string.Empty;

    [JsonPropertyName("pending")]
    public string Pending { get; set; } = string.Empty;

    [JsonPropertyName("failure")]
    public string Failure { get; set; } = string.Empty;
}

public class MercadoPagoPreferenceResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("init_point")]
    public string InitPoint { get; set; } = string.Empty;

    [JsonPropertyName("sandbox_init_point")]
    public string SandboxInitPoint { get; set; } = string.Empty;
}