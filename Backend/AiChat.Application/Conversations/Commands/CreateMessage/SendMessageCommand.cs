namespace AiChat.Application.Conversations.Commands.CreateMessage
{
    public record SendMessageCommand(Guid ConversationId,Guid UserId, string Message);
}
