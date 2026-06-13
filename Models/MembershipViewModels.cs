using System.ComponentModel.DataAnnotations;

namespace grupomathias.Models;

public class MembershipPageViewModel
{
    public string PlanName { get; set; } = "Bike Plus";

    public decimal MonthlyPrice { get; set; } = 19.90m;

    public string Currency { get; set; } = "PEN";

    public string BenefitsSummary { get; set; } = string.Empty;

    public string ApiHint { get; set; } = string.Empty;
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
    public MercadoPagoPreferenceItem[] Items { get; set; } = [];

    public MercadoPagoPayer? Payer { get; set; }

    public string AutoReturn { get; set; } = "approved";

    public MercadoPagoBackUrls BackUrls { get; set; } = new();

    public string ExternalReference { get; set; } = string.Empty;
}

public class MercadoPagoPreferenceItem
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public decimal UnitPrice { get; set; }

    public string CurrencyId { get; set; } = "PEN";
}

public class MercadoPagoPayer
{
    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

public class MercadoPagoBackUrls
{
    public string Success { get; set; } = string.Empty;

    public string Pending { get; set; } = string.Empty;

    public string Failure { get; set; } = string.Empty;
}

public class MercadoPagoPreferenceResponse
{
    public string Id { get; set; } = string.Empty;

    public string InitPoint { get; set; } = string.Empty;

    public string SandboxInitPoint { get; set; } = string.Empty;
}