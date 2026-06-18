namespace AiChat.Application.Dtos
{
    public class ConversationListItemDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public string LastMessage { get; set; } = "";
    
    }
}
