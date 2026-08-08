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

        public async Task<Guid> HandleAsync(CreateConversationCommand createConversationCommand, CancellationToken ct = default)
        {            
            var conversation =Conversation.CreateConversation(createConversationCommand.UserId, createConversationCommand.Username, createConversationCommand.Title);

            await _repository.AddAsync(conversation, ct);

            await _repository.SaveChangesAsync(ct);

            return conversation.Id;
        }
    }
}
