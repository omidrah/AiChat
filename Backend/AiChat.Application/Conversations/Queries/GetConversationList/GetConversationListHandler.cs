using AiChat.Application.Abstractions;
using AiChat.Application.Conversations.Dtos;

namespace AiChat.Application.Conversations.Queries.GetConversationList
{
    public class GetConversationListHandler
    {
        private readonly IConversationRepository _repository;

        public GetConversationListHandler(IConversationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ConversationListItemDto>> HandleAsync(GetConversationListQuery getConversationList, CancellationToken ct = default)
        {
            var conversations =
                await _repository.GetAllConversationsForUserAsync(getConversationList.UserId, ct);

            return conversations
                .Select(x =>
                    new ConversationListItemDto
                    {
                        Id = x.Id,
                        Title = x.Title,
                        CreatedAt = x.CreatedAt,
                        LastMessage = x.Messages.LastOrDefault()?.Content ?? string.Empty,
                    })
                .ToList();
        }
    }
}
