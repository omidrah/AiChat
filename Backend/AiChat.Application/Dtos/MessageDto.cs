using AiChat.Domain.ValueObject;

namespace AiChat.Application.Dtos
{
    public class MessageDto
    {
        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }
}
