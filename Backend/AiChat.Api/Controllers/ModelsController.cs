using AiChat.Application.Abstractions;
using AiChat.Application.Common.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AiChat.Api.Controllers;


[ApiController]
[Route("api/models")]
public class ModelsController : ControllerBase
{
    private readonly IOllamaService _ollamaService;

    public ModelsController(IOllamaService ollamaService)
    {
        _ollamaService = ollamaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ModelDto>>> GetModels(CancellationToken cancellationToken)
    {
        var models = await _ollamaService.GetAvailableModelsAsync(cancellationToken);
        return Ok(models);
    }
}

