namespace AiChat.Application.Abstractions;

public interface IChatCancellationTracker
{
    CancellationToken Register(string conversationId);
    void Cancel(string conversationId);
    void Remove(string conversationId);
}