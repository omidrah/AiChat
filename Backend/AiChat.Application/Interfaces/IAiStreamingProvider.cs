using AiChat.Application.Dtos;

namespace AiChat.Application.Interfaces
{
    public interface IAiStreamingProvider
    {
        Task StreamAsync(IEnumerable<MessageDto> messages, Func<string, Task> onChunk);
    }
}
