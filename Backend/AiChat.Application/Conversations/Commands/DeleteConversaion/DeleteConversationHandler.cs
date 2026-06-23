using AiChat.Application.Abstractions;

namespace AiChat.Application.Conversations.Commands.DeleteConversaion
{
    public class DeleteConversationHandler
    {
        private readonly IConversationRepository _repository;

        public DeleteConversationHandler(IConversationRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> HandleAsync(DeleteConversation command, CancellationToken ct = default)
        {
            var result =
                await _repository.DeleteConversationByUserAsync(command.ConversationId, command.UserId, ct);

            if (result)
            {
                await _repository.SaveChangesAsync();
            }
            return result;
        }
    }
}
