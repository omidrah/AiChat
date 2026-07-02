namespace AiChat.Application.Conversations.Commands.RenameConversation
{
    public record RenameConversationCommand(
    Guid conversationId,
    Guid userId,
    string newtitle);
}
