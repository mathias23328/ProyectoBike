using System.ComponentModel.DataAnnotations;
using Microsoft.ML.Data;

namespace grupomathias.Models;

public class RouteAnalysisRequest
{
    [Required]
    public string Origin { get; set; } = string.Empty;

    [Required]
    public string Destination { get; set; } = string.Empty;

    [Range(1, 60)]
    public int DistanceKm { get; set; }

    [Required]
    public string RiderType { get; set; } = string.Empty;

    [Required]
    public string DepartureWindow { get; set; } = string.Empty;

    public bool PreferBikeLanes { get; set; }

    public bool AvoidHighTraffic { get; set; }

    public bool PreferWellLitAreas { get; set; }
}

public class RouteAnalysisResponse
{
    public string RouteId { get; set; } = string.Empty;

    public bool IsSafeRoute { get; set; }

    public float SafetyProbability { get; set; }

    public int SafetyScore { get; set; }

    public string Recommendation { get; set; } = string.Empty;

    public string AgentReasoning { get; set; } = string.Empty;

    public List<AgentStep> AgentSteps { get; set; } = [];
}

public class AgentStep
{
    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Detail { get; set; } = string.Empty;
}

public class AgentStatusResponse
{
    public string ModelState { get; set; } = string.Empty;

    public string CacheBackend { get; set; } = string.Empty;

    public int RecentRouteCount { get; set; }

    public string LastPredictionSummary { get; set; } = string.Empty;
}

public class RouteTrainingInput
{
    public bool Label { get; set; }

    public float DistanceKm { get; set; }

    public float PreferBikeLanes { get; set; }

    public float AvoidHighTraffic { get; set; }

    public float PreferWellLitAreas { get; set; }

    public float IsNight { get; set; }

    public float IsFamily { get; set; }

    public float IsTraining { get; set; }
}

public class RouteTrainingPrediction
{
    [ColumnName("PredictedLabel")]
    public bool IsSafeRoute { get; set; }

    public float Probability { get; set; }

    public float Score { get; set; }
}