using AiChat.Application.Abstractions;

namespace AiChat.Application.Conversations.Commands.RenameConversation
{
    public class RenameConversationHandler
    {
        private readonly IConversationRepository _repository;

        public RenameConversationHandler(IConversationRepository repository)
        {
            _repository = repository;
        }

        public async Task HandleAsync(RenameConversationCommand command, CancellationToken ct = default)
        {
            await _repository.RenameAsync(command.conversationId,command.userId,command.newtitle,ct);

            await _repository.SaveChangesAsync();
        }
    }
}
