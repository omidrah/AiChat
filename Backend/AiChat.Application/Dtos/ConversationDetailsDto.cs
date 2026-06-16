namespace AiChat.Application.Dtos
{
    public class ConversationDetailsDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = "";

        public List<MessageDto> Messages { get; set; } = [];
    }
}
