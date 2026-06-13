using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using grupomathias.Data;
using grupomathias.Models;

namespace grupomathias.Services;

public interface IMercadoPagoMembershipService
{
    Task<MembershipCheckoutResult> CreateCheckoutAsync(
        string userId,
        MembershipCheckoutRequest request,
        CancellationToken cancellationToken = default);

    MercadoPagoOptions GetOptions();
}

public class MercadoPagoMembershipService : IMercadoPagoMembershipService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MercadoPagoMembershipService> _logger;

    public MercadoPagoMembershipService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ApplicationDbContext dbContext,
        ILogger<MercadoPagoMembershipService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _dbContext = dbContext;
        _logger = logger;
    }

    public MercadoPagoOptions GetOptions()
    {
        return _configuration.GetSection("MercadoPago").Get<MercadoPagoOptions>() ?? new MercadoPagoOptions();
    }

    public async Task<MembershipCheckoutResult> CreateCheckoutAsync(
        string userId,
        MembershipCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = GetOptions();
        if (string.IsNullOrWhiteSpace(options.AccessToken))
        {
            return new MembershipCheckoutResult
            {
                Success = false,
                Message = "Mercado Pago no está configurado en este entorno local."
            };
        }

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);

        var payload = new MercadoPagoPreferencePayload
        {
            ExternalReference = request.FullName,
            Payer = new MercadoPagoPayer
            {
                Email = request.Email,
                Name = request.FullName
            },
            Items = new[]
            {
                new MercadoPagoPreferenceItem
                {
                    Title = request.PlanName,
                    Description = "Membresía Bike Plus para rutas seguras, analítica y soporte premium.",
                    Quantity = 1,
                    UnitPrice = request.MonthlyPrice,
                    CurrencyId = "PEN"
                }
            },
            BackUrls = new MercadoPagoBackUrls
            {
                Success = "https://localhost:5001/membresia/resultado?state=success",
                Pending = "https://localhost:5001/membresia/resultado?state=pending",
                Failure = "https://localhost:5001/membresia/resultado?state=failure"
            }
        };

        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(payload, jsonOptions);
        _logger.LogInformation("MercadoPago payload: {Payload}", json);

        var response = await client.PostAsync(
            "https://api.mercadopago.com/checkout/preferences",
            new StringContent(json, Encoding.UTF8, "application/json"),
            cancellationToken);

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogInformation("MercadoPago response ({Status}): {Response}", response.StatusCode, responseJson);

        if (!response.IsSuccessStatusCode)
        {
            return new MembershipCheckoutResult
            {
                Success = false,
                Message = $"Mercado Pago respondió con error: {response.StatusCode} - {responseJson}",
                PreferenceId = null
            };
        }

        var preference = JsonSerializer.Deserialize<MercadoPagoPreferenceResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (preference?.Id == null)
        {
            return new MembershipCheckoutResult
            {
                Success = false,
                Message = "Respuesta inválida de Mercado Pago."
            };
        }

        // Guardar membresía en BD
        var membership = new UserMembership
        {
            UserId = userId,
            PlanName = request.PlanName,
            MonthlyPrice = request.MonthlyPrice,
            PreferenceId = preference.Id,
            Email = request.Email,
            FullName = request.FullName,
            Status = "pending",
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMonths(1)
        };

        _dbContext.UserMemberships.Add(membership);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new MembershipCheckoutResult
        {
            Success = true,
            Message = "Checkout de membresía generado correctamente.",
            PreferenceId = preference.Id,
            CheckoutUrl = preference.InitPoint,
            SandboxUrl = preference.SandboxInitPoint
        };
    }
}