using grupomathias.Data;
using grupomathias.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace grupomathias.Services;

public interface IBikeRouteAgentService
{
    Task<RouteAnalysisResponse> AnalyzeAsync(RouteAnalysisRequest request, CancellationToken cancellationToken = default);

    Task<AgentStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default);
}

public class BikeRouteAgentService : IBikeRouteAgentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly IHostEnvironment _environment;
    private readonly Lazy<ITransformer> _model;
    private readonly MLContext _mlContext = new(seed: 42);

    public BikeRouteAgentService(ApplicationDbContext dbContext, IDistributedCache cache, IHostEnvironment environment)
    {
        _dbContext = dbContext;
        _cache = cache;
        _environment = environment;
        _model = new Lazy<ITransformer>(TrainModel, true);
    }

    public async Task<RouteAnalysisResponse> AnalyzeAsync(RouteAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        var features = new RouteTrainingInput
        {
            DistanceKm = request.DistanceKm / 60f,
            PreferBikeLanes = request.PreferBikeLanes ? 1f : 0f,
            AvoidHighTraffic = request.AvoidHighTraffic ? 1f : 0f,
            PreferWellLitAreas = request.PreferWellLitAreas ? 1f : 0f,
            IsNight = request.DepartureWindow.Equals("Noche", StringComparison.OrdinalIgnoreCase) ? 1f : 0f,
            IsFamily = request.RiderType.Equals("familiar", StringComparison.OrdinalIgnoreCase) ? 1f : 0f,
            IsTraining = request.RiderType.Equals("entrenamiento", StringComparison.OrdinalIgnoreCase) ? 1f : 0f
        };

        var engine = _mlContext.Model.CreatePredictionEngine<RouteTrainingInput, RouteTrainingPrediction>(_model.Value);
        var prediction = engine.Predict(features);

        var safetyScore = Math.Clamp((int)Math.Round(prediction.Probability * 100), 0, 100);
        var routeId = $"route-{Guid.NewGuid():N}"[..13];
        var safeRoute = prediction.Probability >= 0.5f;

        var recentCount = await _dbContext.BikeRouteLogs.CountAsync(cancellationToken);
        var steps = BuildAgentSteps(request, safetyScore, recentCount);
        var reasoning = safeRoute
            ? "El modelo favorece este trayecto por sus señales de menor exposición vial y mejor control urbano."
            : "El modelo detecta factores de riesgo que conviene compensar con una ruta más corta o con más ciclovías.";

        await _cache.SetStringAsync(
            "bike:agent:last-analysis",
            $"{request.Origin} -> {request.Destination} | {safetyScore}%",
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) },
            cancellationToken);

        _dbContext.BikeRouteLogs.Add(new BikeRouteLog
        {
            Origin = request.Origin,
            Destination = request.Destination,
            DistanceKm = request.DistanceKm,
            RiderType = request.RiderType,
            SafetyScore = safetyScore,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RouteAnalysisResponse
        {
            RouteId = routeId,
            IsSafeRoute = safeRoute,
            SafetyProbability = prediction.Probability,
            SafetyScore = safetyScore,
            Recommendation = safeRoute
                ? "Ruta recomendada por el agente ML."
                : "Ruta posible, pero requiere ajustes para elevar seguridad.",
            AgentReasoning = reasoning,
            AgentSteps = steps
        };
    }

    public async Task<AgentStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var recentCount = await _dbContext.BikeRouteLogs.CountAsync(cancellationToken);
        var lastPrediction = await _cache.GetStringAsync("bike:agent:last-analysis", cancellationToken) ?? "Sin predicción previa.";

        return new AgentStatusResponse
        {
            ModelState = _model.IsValueCreated ? "Modelo ML entrenado en memoria." : "Modelo ML listo para entrenarse bajo demanda.",
            CacheBackend = _environment.IsDevelopment() ? "Memory cache local" : "Redis o memory cache según configuración",
            RecentRouteCount = recentCount,
            LastPredictionSummary = lastPrediction
        };
    }

    private ITransformer TrainModel()
    {
        var trainingData = new List<RouteTrainingInput>
        {
            new() { DistanceKm = 0.10f, PreferBikeLanes = 1, AvoidHighTraffic = 1, PreferWellLitAreas = 1, IsNight = 0, IsFamily = 1, IsTraining = 0, Label = true },
            new() { DistanceKm = 0.18f, PreferBikeLanes = 1, AvoidHighTraffic = 1, PreferWellLitAreas = 1, IsNight = 0, IsFamily = 0, IsTraining = 0, Label = true },
            new() { DistanceKm = 0.30f, PreferBikeLanes = 1, AvoidHighTraffic = 1, PreferWellLitAreas = 1, IsNight = 1, IsFamily = 1, IsTraining = 0, Label = true },
            new() { DistanceKm = 0.45f, PreferBikeLanes = 1, AvoidHighTraffic = 0, PreferWellLitAreas = 1, IsNight = 0, IsFamily = 0, IsTraining = 1, Label = true },
            new() { DistanceKm = 0.52f, PreferBikeLanes = 1, AvoidHighTraffic = 1, PreferWellLitAreas = 0, IsNight = 0, IsFamily = 0, IsTraining = 1, Label = true },
            new() { DistanceKm = 0.65f, PreferBikeLanes = 0, AvoidHighTraffic = 1, PreferWellLitAreas = 1, IsNight = 0, IsFamily = 1, IsTraining = 0, Label = true },
            new() { DistanceKm = 0.68f, PreferBikeLanes = 1, AvoidHighTraffic = 1, PreferWellLitAreas = 1, IsNight = 1, IsFamily = 0, IsTraining = 1, Label = true },
            new() { DistanceKm = 0.82f, PreferBikeLanes = 0, AvoidHighTraffic = 0, PreferWellLitAreas = 0, IsNight = 1, IsFamily = 0, IsTraining = 1, Label = false },
            new() { DistanceKm = 0.88f, PreferBikeLanes = 0, AvoidHighTraffic = 0, PreferWellLitAreas = 1, IsNight = 1, IsFamily = 0, IsTraining = 0, Label = false },
            new() { DistanceKm = 0.76f, PreferBikeLanes = 0, AvoidHighTraffic = 0, PreferWellLitAreas = 0, IsNight = 0, IsFamily = 0, IsTraining = 1, Label = false },
            new() { DistanceKm = 0.92f, PreferBikeLanes = 0, AvoidHighTraffic = 1, PreferWellLitAreas = 0, IsNight = 1, IsFamily = 0, IsTraining = 0, Label = false },
            new() { DistanceKm = 0.95f, PreferBikeLanes = 0, AvoidHighTraffic = 0, PreferWellLitAreas = 0, IsNight = 1, IsFamily = 1, IsTraining = 0, Label = false },
            new() { DistanceKm = 0.70f, PreferBikeLanes = 0, AvoidHighTraffic = 0, PreferWellLitAreas = 1, IsNight = 0, IsFamily = 0, IsTraining = 1, Label = false },
            new() { DistanceKm = 0.85f, PreferBikeLanes = 1, AvoidHighTraffic = 0, PreferWellLitAreas = 0, IsNight = 1, IsFamily = 0, IsTraining = 0, Label = false },
            new() { DistanceKm = 0.40f, PreferBikeLanes = 1, AvoidHighTraffic = 1, PreferWellLitAreas = 0, IsNight = 0, IsFamily = 1, IsTraining = 0, Label = true },
            new() { DistanceKm = 0.58f, PreferBikeLanes = 1, AvoidHighTraffic = 1, PreferWellLitAreas = 1, IsNight = 0, IsFamily = 1, IsTraining = 0, Label = true }
        };

        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);
        var pipeline = _mlContext.Transforms.Concatenate(
                "Features",
                nameof(RouteTrainingInput.DistanceKm),
                nameof(RouteTrainingInput.PreferBikeLanes),
                nameof(RouteTrainingInput.AvoidHighTraffic),
                nameof(RouteTrainingInput.PreferWellLitAreas),
                nameof(RouteTrainingInput.IsNight),
                nameof(RouteTrainingInput.IsFamily),
                nameof(RouteTrainingInput.IsTraining))
            .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: nameof(RouteTrainingInput.Label), featureColumnName: "Features"));

        return pipeline.Fit(dataView);
    }

    private static List<AgentStep> BuildAgentSteps(RouteAnalysisRequest request, int safetyScore, int recentCount)
    {
        return new List<AgentStep>
        {
            new()
            {
                Name = "Contexto",
                Status = "done",
                Detail = $"Se recibió una ruta de {request.Origin} a {request.Destination} con {request.DistanceKm} km."
            },
            new()
            {
                Name = "Predicción ML",
                Status = "done",
                Detail = $"El modelo estimó una seguridad de {safetyScore}% sobre la base de las preferencias capturadas."
            },
            new()
            {
                Name = "Memoria operativa",
                Status = "done",
                Detail = recentCount > 0
                    ? $"Se encontró historial reciente de {recentCount} rutas guardadas en la base de datos."
                    : "Todavía no hay historial para contrastar la recomendación."
            },
            new()
            {
                Name = "Acción del agente",
                Status = safetyScore >= 70 ? "recommended" : "review",
                Detail = safetyScore >= 70
                    ? "La ruta puede recomendarse y guardarse como opción prioritaria."
                    : "El agente sugiere reforzar ciclovías, iluminación o reducir distancia."
            }
        };
    }
}