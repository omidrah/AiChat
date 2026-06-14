using AiChat.Application.Dtos;

namespace AiChat.Infrastructure.AI
{
    public class OllamaChatResponse
    {
        public MessageDto Message { get; set; } = new();
    }
}
