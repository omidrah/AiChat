using AiChat.Application.Conversations.Dtos;

namespace AiChat.Application.Abstractions
{
    public interface IAiProvider
    {
        Task<string> AskAsync(IEnumerable<MessageDto> messages);
    }
}
