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

        public async Task<bool> HandleAsync(DeleteConversation command)
        {
            var conversation =
                await _repository.GetAsync(command.ConversationId);

            if (conversation is null)
                return false;

            await _repository.DeleteAsync(conversation);

            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
