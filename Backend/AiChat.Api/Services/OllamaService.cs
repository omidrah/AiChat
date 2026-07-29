namespace AiChat.Api.Services;

using AiChat.Application.Abstractions;
using AiChat.Application.Common.Dtos;
using AiChat.Infrastructure.AI;
using Microsoft.Extensions.Options;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaService(HttpClient httpClient, IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    //GET /api/models لیست مدلهای موجود روی ollama

    public async Task<List<ModelDto>> GetAvailableModelsAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<OllamaTagsResponse>(
            $"{_options.BaseUrl}/api/tags",
            cancellationToken);

        Console.WriteLine(response);

        return response?.Models?
            .Select(x => new ModelDto(x.Name))
            .ToList()
            ?? new List<ModelDto>();
    }
}
