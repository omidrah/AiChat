using AiChat.Application.Common.Dtos;

namespace AiChat.Application.Abstractions;

public interface IOllamaService
{
    Task<List<ModelDto>> GetAvailableModelsAsync(CancellationToken cancellationToken);
}
