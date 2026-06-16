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

        public async Task HandleAsync(RenameConversationCommand command)
        {
            var conversation = await _repository.GetAsync(command.conversationId);

            if (conversation is null)
                throw new Exception("Not Found");

            conversation.Rename(command.newTitle);

            await _repository.SaveChangesAsync();
        }
    }
}
