using AiChat.Domain.ValueObject;

namespace AiChat.Domain.Entities
{
    public class Message
    {
        public Guid Id { get; private set; }

        public MessageRole Role { get; private set; }

        public string Content { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public Guid ConversationId { get; private set; }

        public string? Model { get;  private set; }

        private Message(Guid conversationId, MessageRole role, string content, string model)
        {
            Id = Guid.NewGuid();
            ConversationId = conversationId;
            Role = role;
            Content = content;
            CreatedAt = DateTime.UtcNow;  
            Model = model;
        }

        public static Message CreateMessage(Guid conversationId, MessageRole role, string content, string aiModel)
        {
           return new Message(conversationId,role, content, aiModel);  
        }
    }
}
