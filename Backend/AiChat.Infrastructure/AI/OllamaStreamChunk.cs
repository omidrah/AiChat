using System.Text.Json.Serialization;

namespace AiChat.Infrastructure.AI
{
    public class OllamaStreamChunk
    {
        [JsonPropertyName("message")]
        public OllamaStreamMessage Message { get; set; }   = new();

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }
}