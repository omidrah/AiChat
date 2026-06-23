using AiChat.Application.Abstractions;
using AiChat.Application.Conversations.Dtos;
using Microsoft.Extensions.Options;
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
        private readonly OllamaOptions _options;

        public OllamaStreamingProvider(HttpClient client, IOptions<OllamaOptions> options)
        {
            _client = client;
            _options = options.Value;
            _client.BaseAddress = new Uri(_options.BaseUrl);
        }

        public async Task<string> AskAsync(IEnumerable<MessageDto> messages)
        {
            var newrequest = new OllamaChatRequest
            {
                Model = _options.Model,
                Messages = messages.ToList(),
            };

            //var json = System.Text.Json.JsonSerializer.Serialize(newrequest,
            //                 new JsonSerializerOptions
            //                 {
            //                     WriteIndented = true
            //                 });

            //Console.WriteLine(json);

            var response = await _client.PostAsJsonAsync("/api/chat", newrequest);

            if (!response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();

                throw new Exception(
                    $"Ollama error. StatusCode: {(int)response.StatusCode} {response.StatusCode}. Body: {responseText}"
                );
            }

            var result =
                await response.Content.ReadFromJsonAsync<OllamaChatResponse>();

            return result?.Message.Content ?? "";
        }
        public async Task StreamAsync(IEnumerable<MessageDto> messages, Func<string, Task> onChunk, CancellationToken ct)
        {
            var request = new
            {
                model = _options.Model,
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
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line =
                    await reader.ReadLineAsync();

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
