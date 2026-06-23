namespace AiChat.Application.Conversations.Dtos
{
    public class MessageDto
    {
        public Guid Id { get; set; }

        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

    }
}
