using AiChat.Application.Abstractions;
using AiChat.Domain.Entities;

namespace AiChat.Application.Conversations.Commands.CreateConversation
{
    public class CreateConversationHandler
    {
        private readonly IConversationRepository _repository;

        public CreateConversationHandler(IConversationRepository repository)
        {
            _repository = repository;
        }

        public async Task<Guid> HandleAsync(CreateConversation command, CancellationToken cancellationToken = default)
        {
            var conversation =Conversation.CreateConversation(command.Title);

            await _repository.AddAsync(conversation, cancellationToken);

            await _repository.SaveChangesAsync(cancellationToken);

            return conversation.Id;
        }
    }
}
