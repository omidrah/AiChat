using AiChat.Domain.ValueObject;

namespace AiChat.Domain.Entities
{
    public class Conversation
    {
        private readonly List<Message> _messages = [];

        public Guid Id { get; private set; }

        public string Title { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

        private Conversation(string title)
        {
            Id = Guid.NewGuid();

            Title = title;

            CreatedAt = DateTime.UtcNow;
        }

        public static Conversation CreateConversation(string title)
        {
            return new Conversation(title);            
        }

        public void AddMessage(string content,MessageRole role)
        {
            var message =Message.CreateMessage(Id, role, content);
            _messages.Add(message);
        }
        public void Rename(string title)
        {
            Title = title;
        }
    }
}
