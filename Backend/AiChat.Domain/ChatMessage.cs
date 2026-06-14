namespace AiChat.Domain
{
    public class ChatMessage
    {
        public Guid Id { get; set; }

        public string Role { get; set; } = "";

        public string Content { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
