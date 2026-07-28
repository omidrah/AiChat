namespace AiChat.Api.Services;

using AiChat.Application.Abstractions;
using AiChat.Application.Common.Dtos;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;

    public OllamaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    //GET /api/models لیست مدلهای موجود روی ollama
    public async Task<List<ModelDto>> GetAvailableModelsAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<OllamaTagsResponse>(
            "http://localhost:11434/api/tags",
            cancellationToken);

        return response?.Models?
            .Select(x => new ModelDto(x.Name))
            .ToList()
            ?? new List<ModelDto>();
    }
}
