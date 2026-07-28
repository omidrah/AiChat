namespace AiChat.Application.Abstractions
{
    public interface IConversationTitleGenerator
    {
        Task<string> GenerateTitleAsync(
            string firstMessage, string model, CancellationToken ct);
    }
}
