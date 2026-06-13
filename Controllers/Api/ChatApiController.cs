using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using grupomathias.Models;
using grupomathias.Services;

namespace grupomathias.Controllers.Api
{
    [ApiController]
    [Route("api/chat")]
    public class ChatApiController : ControllerBase
    {
        private readonly IBikeRouteAgentService _agentService;

        public ChatApiController(IBikeRouteAgentService agentService)
        {
            _agentService = agentService;
        }

        public class ChatRequest
        {
            public string Question { get; set; }
        }

        public class ChatResponse
        {
            public object Status { get; set; }
            public object Analysis { get; set; }
        }

        [HttpPost("recommend")]
        public async Task<IActionResult> Recommend([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Question))
                return BadRequest(new { error = "Question is required" });

            var status = await _agentService.GetStatusAsync();

            var analysisReq = new RouteAnalysisRequest
            {
                Origin = request.Question,
                Destination = "",
                DistanceKm = 5,
                RiderType = "urbano",
                DepartureWindow = "Cualquier",
                PreferBikeLanes = true,
                AvoidHighTraffic = false,
                PreferWellLitAreas = true
            };

            var analysis = await _agentService.AnalyzeAsync(analysisReq);

            var resp = new ChatResponse { Status = status, Analysis = analysis };
            return Ok(resp);
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { ok = true, name = "chat-api" });
        }
    }
}
