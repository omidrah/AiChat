namespace AiChat.Application.Conversations.Commands
{
    public record SendMessageCommand( Guid ConversationId,  string Message);
}
