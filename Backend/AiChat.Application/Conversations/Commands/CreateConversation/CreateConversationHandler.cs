using AiChat.Application.Abstractions;
using AiChat.Application.Common.Auth;
using AiChat.Domain.Entities;

namespace AiChat.Application.Conversations.Commands.CreateConversation
{
    public class CreateConversationHandler
    {
        private readonly IConversationRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public CreateConversationHandler(IConversationRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<Guid> HandleAsync(CreateConversationCommand createConversationCommand, CancellationToken ct = default)
        {            
            var conversation =Conversation.CreateConversation(createConversationCommand.UserId,createConversationCommand.Username, createConversationCommand.Title);

            await _repository.AddAsync(conversation, ct);

            await _repository.SaveChangesAsync(ct);

            return conversation.Id;
        }
    }
}
