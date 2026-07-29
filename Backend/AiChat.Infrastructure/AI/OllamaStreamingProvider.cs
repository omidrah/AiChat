using AiChat.Application.Abstractions;
using AiChat.Application.Conversations.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace AiChat.Infrastructure.AI
{
    /// <summary>
    /// But Model provider (ex.ChatGPT) does not return the entire response together, but rather sends the response in pieces:
    //  use this provider for stream pieces from Model provider
    /// </summary>
    public class OllamaStreamingProvider : IAiStreamingProvider
    {
        private readonly HttpClient _client;
        private readonly ILogger<OllamaStreamingProvider> logger;
        private readonly OllamaOptions _options;

        public OllamaStreamingProvider(HttpClient client, IOptions<OllamaOptions> options, ILogger<OllamaStreamingProvider> logger)
        {
            _client = client;
            this.logger = logger;
            _options = options.Value;
            _client.BaseAddress = new Uri(_options.BaseUrl);
        }

        public async Task<string> AskAsync(IEnumerable<MessageDto> messages, string model, CancellationToken ct)
        {
            var newrequest = new OllamaChatRequest
            {
                Model = !string.IsNullOrEmpty(model) ? model : _options.Model,
                Messages = messages.ToList(),
            };

            //var json = System.Text.Json.JsonSerializer.Serialize(newrequest,
            //                 new JsonSerializerOptions
            //                 {
            //                     WriteIndented = true
            //                 });

            //Console.WriteLine(json);

            try
            {
                var response = await _client.PostAsJsonAsync("/api/chat", newrequest, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var responseText = await response.Content.ReadAsStringAsync(ct);

                    throw new Exception(
                        $"Ollama error. StatusCode: {(int)response.StatusCode} {response.StatusCode}. Body: {responseText}"
                    );
                }
                var result =   await response.Content.ReadFromJsonAsync<OllamaChatResponse>(ct);

                return result?.Message.Content ?? "";
            }

            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Failed to communicate with Ollama service after retries.");

                // پرتاب خطای استاندارد با استاتوس کد 503 جهت پردازش در GlobalExceptionHandler
                throw new HttpRequestException("Ollama Service is currently unavailable.", ex, HttpStatusCode.ServiceUnavailable);
            }
           
        }
        public async Task StreamAsync(IEnumerable<MessageDto> messages, string? model, Func<string, Task> onChunk, CancellationToken ct)
        {
            var request = new
            {
                model = !string.IsNullOrEmpty(model) ? model : _options.Model,
                messages = messages,
                stream = true
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/chat");

            httpRequest.Content = JsonContent.Create(request);

            var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
            // response.EnsureSuccessStatusCode();
            if (!response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync(ct);

                throw new Exception(
                    $"Ollama error. StatusCode: {(int)response.StatusCode} {response.StatusCode}. Body: {responseText}"
                );
            }
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line =
                    await reader.ReadLineAsync(ct);

                if (string.IsNullOrWhiteSpace(line))
                    continue;
                // Console.WriteLine($"LINE={line}");

                await ProcessChunk(line, onChunk);
            }
        }
        private static async Task ProcessChunk(string json, Func<string, Task> onChunk)
        {
            // Console.WriteLine($"JSON={json}");

            try
            {
                var chunk = JsonSerializer.Deserialize<OllamaStreamChunk>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Console.WriteLine($"CONTENT={chunk?.Message?.Content}");
                if (!string.IsNullOrEmpty(chunk?.Message?.Content))
                {
                    if (chunk.Message.Content.Contains("<think>") || chunk.Message.Content.Contains("</think>"))
                    {
                        return;
                    }
                    await onChunk(chunk.Message.Content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
