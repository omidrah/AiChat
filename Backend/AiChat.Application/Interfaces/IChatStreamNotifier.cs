namespace AiChat.Application.Interfaces
{
    public interface IChatStreamNotifier
    {
        Task SendChunkAsync(
            Guid conversationId,
            string chunk);
    }
}
