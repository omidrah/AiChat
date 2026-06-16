using AiChat.Application.Abstractions;
using AiChat.Application.Dtos;

namespace AiChat.Infrastructure.AI
{
    public class OllamaConversationTitleGenerator
     : IConversationTitleGenerator
    {
        private readonly IAiProvider _provider;

        public OllamaConversationTitleGenerator(
            IAiProvider provider)
        {
            _provider = provider;
        }

        public async Task<string> GenerateTitleAsync(
            string firstMessage)
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

            return await _provider.AskAsync(messages);
        }
    }
}
