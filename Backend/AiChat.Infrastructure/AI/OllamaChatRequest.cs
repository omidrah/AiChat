using AiChat.Application.Conversations.Dtos;

namespace AiChat.Infrastructure.AI
{
    public class OllamaChatRequest
    {
        public string Model { get; set; } = "";

        public IEnumerable<MessageDto> Messages { get; set; } = [];
        public bool Stream { get; set; } = false;

    }
}
