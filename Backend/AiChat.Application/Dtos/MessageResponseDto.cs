namespace AiChat.Application.Dtos
{
    public class MessageResponseDto
    {
        public Guid Id { get; set; }

        public string Role { get; set; } = "";

        public string Content { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}
