using AiChat.Application.Abstractions;
using AiChat.Application.Dtos;

namespace AiChat.Application.Conversations.Queries.GetConversationList
{
    public class GetConversationListHandler
    {
        private readonly IConversationRepository _repository;

        public GetConversationListHandler(IConversationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ConversationListItemDto>> HandleAsync()
        {
            var conversations =
                await _repository.GetAllAsync();

            return conversations
                .Select(x =>
                    new ConversationListItemDto
                    {
                        Id = x.Id,
                        Title = x.Title,
                        CreatedAt = x.CreatedAt
                    })
                .ToList();
        }
    }
}
