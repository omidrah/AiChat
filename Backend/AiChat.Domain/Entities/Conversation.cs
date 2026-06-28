using AiChat.Domain.ValueObject;

namespace AiChat.Domain.Entities
{
    public class Conversation
    {
        private readonly List<Message> _messages = [];

        public Guid Id { get; private set; }

        public Guid UserId { get;  private set; }

        public string UserName { get; private set; } 
        public string Title { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

        private Conversation(Guid userId,string userName, string title)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Title = title;
            UserName = userName;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;    
        }

        public static Conversation CreateConversation(Guid userId,string userName, string title)
        {
            return new Conversation(userId, userName,title);            
        }

        public void AddMessage(string content,MessageRole role)
        {
            var message =Message.CreateMessage(Id, role, content);
            _messages.Add(message);
            UpdatedAt = DateTime.UtcNow;
        }
        public void Rename(string title)
        {
            Title = title;
        }
    }
}
