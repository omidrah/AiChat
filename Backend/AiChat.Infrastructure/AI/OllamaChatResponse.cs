using AiChat.Application.Conversations.Dtos;

namespace AiChat.Infrastructure.AI
{
    public class OllamaChatResponse
    {
        public MessageDto Message { get; set; } = new();
    }
}
