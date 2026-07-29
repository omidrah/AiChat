using AiChat.Application.Abstractions;
using AiChat.Application.Common.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.Api.Controllers;

[ApiController]
[Route("api/health")]
public class AiHealthController : ControllerBase
{
    private readonly IAiHealthService _aiHealthService;

    public AiHealthController(IAiHealthService aiHealthService)
    {
        _aiHealthService = aiHealthService;
    }

    [HttpGet("ai")]
    public async Task<IActionResult> GetAiHealth()
    {
        var health = await _aiHealthService.CheckHealthAsync();
        if (!health.IsHealthy)
        {
            // بازگرداندن وضعیت 503 Service Unavailable به همراه جزئیات برای فرانت
            return StatusCode(503, health);
        }
        return Ok(health);
    }

    [HttpGet("ai/details")]
    public async Task<IActionResult> GetAiDetails()
    {
        var details = await _aiHealthService.GetServerDetailsAsync();
        return Ok(details);
    }

    [HttpGet]
    public async Task<ActionResult<List<ModelDto>>> GetModels(CancellationToken cancellationToken)
    {
        var models = await _aiHealthService.GetAvailableModelsAsync(cancellationToken);
        return Ok(models);
    }

}