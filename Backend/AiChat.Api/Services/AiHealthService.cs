namespace AiChat.Api.Services;

using AiChat.Application.Abstractions;
using AiChat.Application.Common.Ai;
using AiChat.Application.Common.Dtos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

public class AiHealthService : IAiHealthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AiHealthService> _logger;
    private const string CacheKey = "AiHealthStatus";

    public AiHealthService(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<AiHealthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AiHealthStatus> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out AiHealthStatus cachedStatus))
        {
            return cachedStatus;
        }

        var status = new AiHealthStatus { CheckedAt = DateTime.UtcNow };

        try
        {
            var client = _httpClientFactory.CreateClient("OllamaClient");
            // با استفاده از یک Timeout کوتاه برای Health Check تا سرور معطل نشود
            client.Timeout = TimeSpan.FromSeconds(3);

            // متد HEAD یا GET به آدرس روت Ollama یا api/tags
            var response = await client.GetAsync("api/tags", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                status.IsHealthy = true;
                status.Message = "Ollama is responding and healthy.";
            }
            else
            {
                status.IsHealthy = false;
                status.Message = $"Ollama returned status code: {(int)response.StatusCode} {response.ReasonPhrase}";
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Ollama health check failed due to connection error.");
            status.IsHealthy = false;
            status.Message = "Cannot connect to Ollama. Service might be down or port is blocked.";
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Ollama health check timed out.");
            status.IsHealthy = false;
            status.Message = "Connection to Ollama timed out.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Ollama health check.");
            status.IsHealthy = false;
            status.Message = "An unexpected error occurred while checking AI health.";
        }

        // کش کردن وضعیت برای ۱۰ ثانیه جهت جلوگیری از ارسال درخواست‌های تکراری و همزمان
        _cache.Set(CacheKey, status, TimeSpan.FromSeconds(10));

        return status;
    }

    public async Task<OllamaServerDetails> GetServerDetailsAsync(CancellationToken cancellationToken = default)
    {
        var details = new OllamaServerDetails();
        var client = _httpClientFactory.CreateClient("OllamaClient");
        client.Timeout = TimeSpan.FromSeconds(5);

        try
        {
            // ۱. دریافت نسخه سرور Ollama
            var versionResponse = await client.GetAsync("api/version", cancellationToken);
            if (versionResponse.IsSuccessStatusCode)
            {
                var versionData = await versionResponse.Content.ReadFromJsonAsync<JsonNode>(cancellationToken);
                details.Version = versionData?["version"]?.ToString() ?? "Unknown";
                details.IsConnected = true;
            }

            // ۲. دریافت لیست مدل‌ها
            var modelsResponse = await client.GetAsync("api/tags", cancellationToken);
            if (modelsResponse.IsSuccessStatusCode)
            {
                var tagsData = await modelsResponse.Content.ReadFromJsonAsync<JsonNode>(cancellationToken);
                var modelsNode = tagsData?["models"]?.AsArray();
                if (modelsNode != null)
                {
                    foreach (var modelNode in modelsNode)
                    {
                        long bytes = modelNode?["size"]?.GetValue<long>() ?? 0;
                        double gb = bytes / 1024.0 / 1024.0 / 1024.0;

                        var detailsNode = modelNode?["details"];

                        details.Models.Add(new OllamaModelInfo
                        {
                            Name = modelNode?["name"]?.ToString() ?? "Unknown",
                            Size = $"{gb:F2} GB",
                            Format = detailsNode?["format"]?.ToString() ?? "Unknown",
                            Family = detailsNode?["family"]?.ToString() ?? "Unknown",
                            ParameterSize = detailsNode?["parameter_size"]?.ToString() ?? "Unknown"
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve full Ollama server details.");
            details.IsConnected = false;
        }

        return details;
    }

    public async Task<List<ModelDto>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("OllamaClient");
            var response = await client.GetFromJsonAsync<OllamaTagsResponse>("api/tags", cancellationToken);

            return response?.Models?
                .Select(x => new ModelDto(x.Name))
                .ToList()
                ?? new List<ModelDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve available models from Ollama.");
            return new List<ModelDto>();
        }
    }
}

