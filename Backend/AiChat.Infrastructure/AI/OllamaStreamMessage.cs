using System.Text.Json.Serialization;

namespace AiChat.Infrastructure.AI
{
    public class OllamaStreamMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "";

        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
    }
}