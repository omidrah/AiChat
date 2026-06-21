using AiChat.Application.Abstractions;
using AiChat.Application.Dtos;

namespace AiChat.Application.Conversations.Queries.GetConverstaions
{
    public class GetConversationHandler
    {
        private readonly IConversationRepository _repository;

        public GetConversationHandler(IConversationRepository repository)
        {
            _repository = repository;
        }

        public async Task<ConversationDetailsDto?>  HandleAsync(GetConversationQuery query)
        {
            var conversation = await _repository.GetAsync(query.ConversationId);

            if (conversation is null)
                return null;

            return new ConversationDetailsDto
            {
                Id = conversation.Id,
                Title = conversation.Title,
                Messages =
                    conversation.Messages.TakeLast(20).OrderBy(x=> x.CreatedAt)
                        .Select(x =>
                            new MessageDto
                            {
                                Role =
                                    x.Role.ToString()
                                        .ToLower(),

                                Content =
                                    x.Content,
                                CreatedAt = x.CreatedAt,

                                Id = conversation.Id
                            })
                        .ToList()
            };
        }
    }
}
