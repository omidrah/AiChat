using AiChat.Application.Abstractions;
using AiChat.Application.Dtos;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace AiChat.Infrastructure.AI
{
    public class OllamaProvider : IAiProvider
    {
        private readonly HttpClient _client;
        private readonly OllamaOptions _options;

        public OllamaProvider(HttpClient client, IOptions<OllamaOptions> options)
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

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content.ReadFromJsonAsync<OllamaChatResponse>();

            return result?.Message.Content ?? "";
        }
    }
}
