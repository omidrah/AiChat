using AiChat.Application.Conversations.Dtos;

namespace AiChat.Application.Abstractions
{
    public interface IAiStreamingProvider
    {
        Task StreamAsync(IEnumerable<MessageDto> messages, string model, Func<string, Task> onChunk, CancellationToken ct);
        Task<string> AskAsync(IEnumerable<MessageDto> messages, string model, CancellationToken ct);
    }
}
