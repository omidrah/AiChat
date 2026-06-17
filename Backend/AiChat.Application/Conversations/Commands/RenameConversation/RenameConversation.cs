namespace AiChat.Application.Conversations.Commands.RenameConversation
{
    public record RenameConversation(Guid conversationId,string newTitle);
}
