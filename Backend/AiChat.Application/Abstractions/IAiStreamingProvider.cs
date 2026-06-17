using AiChat.Application.Dtos;

namespace AiChat.Application.Abstractions
{
    public interface IAiStreamingProvider
    {
        Task StreamAsync(IEnumerable<MessageDto> messages, Func<string, Task> onChunk, CancellationToken ct);
        Task<string> AskAsync(IEnumerable<MessageDto> messages);
    }
}
