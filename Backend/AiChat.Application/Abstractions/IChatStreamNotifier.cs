namespace AiChat.Application.Abstractions
{
    public interface IChatStreamNotifier
    {
        Task SendChunkAsync(Guid conversationId,string chunk);
        Task CompleteAsync(Guid conversationId);

    }
}
