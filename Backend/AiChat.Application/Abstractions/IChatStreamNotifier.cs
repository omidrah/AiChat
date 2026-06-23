namespace AiChat.Application.Abstractions
{
    public interface IChatStreamNotifier
    {
        Task SendChunkAsync(Guid conversationId, Guid userId, string chunk);
        Task CompleteAsync(Guid conversationId, Guid userId);

    }
}
