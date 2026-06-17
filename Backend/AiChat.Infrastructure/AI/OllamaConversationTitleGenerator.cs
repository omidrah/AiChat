using AiChat.Application.Abstractions;
using AiChat.Application.Dtos;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace AiChat.Infrastructure.AI
{
    public class OllamaConversationTitleGenerator: IConversationTitleGenerator
    {
        private readonly IAiStreamingProvider _provider;

        public OllamaConversationTitleGenerator(IAiStreamingProvider provider)
        {
            _provider = provider;
        }

        public async Task<string> GenerateTitleAsync(string firstMessage)
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
                return await _provider.AskAsync(messages);
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
