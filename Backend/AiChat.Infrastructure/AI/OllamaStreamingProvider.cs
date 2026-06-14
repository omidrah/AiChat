using AiChat.Application.Dtos;
using AiChat.Application.Interfaces;
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

        public async Task StreamAsync(IEnumerable<MessageDto> messages,  Func<string, Task> onChunk)
        {
            var request = new
            {
                model = _options.Model,
                messages = messages,
                stream = true
            };
            Console.WriteLine("1");

            var response =
                await _client.PostAsJsonAsync(
                    "/api/chat",
                    request);

            response.EnsureSuccessStatusCode();
            Console.WriteLine("2");

            await using var stream =   await response.Content.ReadAsStreamAsync();
            Console.WriteLine("3");

            using var reader =  new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line =
                    await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                    continue;
                 Console.WriteLine($"LINE={line}");

                //await ProcessChunk(
                //    line,
                //    onChunk);
            }
        }
        private static async Task ProcessChunk(string json, Func<string, Task> onChunk)
        {
            try
            {
                var chunk =
                    JsonSerializer.Deserialize<OllamaStreamChunk>(json);

                if (!string.IsNullOrEmpty(chunk?.Message?.Content))
                {
                    await onChunk(chunk.Message.Content);
                }
            }
            catch
            {
            }
        }
    }
}
