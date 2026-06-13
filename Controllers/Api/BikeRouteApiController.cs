using grupomathias.Models;
using grupomathias.Services;
using Microsoft.AspNetCore.Mvc;

namespace grupomathias.Controllers.Api;

[ApiController]
[Route("api/bike")]
public class BikeRouteApiController : ControllerBase
{
    private readonly IBikeRouteAgentService _bikeRouteAgentService;

    public BikeRouteApiController(IBikeRouteAgentService bikeRouteAgentService)
    {
        _bikeRouteAgentService = bikeRouteAgentService;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            service = "Proyecto Bike API",
            status = "ok",
            endpoints = new[] { "/api/bike/health", "/api/bike/analyze", "/api/bike/status" }
        });
    }

    [HttpGet("status")]
    public async Task<ActionResult<AgentStatusResponse>> Status(CancellationToken cancellationToken)
    {
        var status = await _bikeRouteAgentService.GetStatusAsync(cancellationToken);
        return Ok(status);
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<RouteAnalysisResponse>> Analyze([FromBody] RouteAnalysisRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var response = await _bikeRouteAgentService.AnalyzeAsync(request, cancellationToken);
        return Ok(response);
    }
}