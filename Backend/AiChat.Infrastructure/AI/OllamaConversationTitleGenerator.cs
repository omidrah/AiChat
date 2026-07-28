using AiChat.Application.Abstractions;
using AiChat.Application.Conversations.Dtos;

namespace AiChat.Infrastructure.AI
{
    public class OllamaConversationTitleGenerator: IConversationTitleGenerator
    {
        private readonly IAiStreamingProvider _provider;

        public OllamaConversationTitleGenerator(IAiStreamingProvider provider)
        {
            _provider = provider;
        }

        public async Task<string> GenerateTitleAsync(string firstMessage, string model, CancellationToken ct=default)
        {
            var messages =
                new List<MessageDto>
                {
                    new()
                    {
                        Role = "system",
                        Content =
                            """
                            Generate a short title.
                            Maximum 5 words.
                            No quotes.
                            No punctuation.
                            """
                    },
                    new()
                    {
                        Role = "user",
                        Content = firstMessage
                    }
                };
            try
            {
                return await _provider.AskAsync(messages, model, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Title generation failed: {ex.Message}");
                return firstMessage.Length > 40
                    ? firstMessage.Substring(0, 40) + "..."
                    : firstMessage;
             }
        }
    }
}
