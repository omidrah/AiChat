namespace AiChat.Application.Conversations.Commands.CreateMessage;

using AiChat.Application.Abstractions;
using AiChat.Application.Dtos;
using AiChat.Domain.Entities;
using System.Text;

public class SendMessageHandler
{
    private readonly IConversationRepository _repository;
    private readonly IAiProvider _aiProvider;
    private readonly IAiStreamingProvider _aiStreamingProvider;
    private readonly IChatStreamNotifier _notifier;
    private readonly IConversationTitleGenerator _titleGenerator;
    public SendMessageHandler(
        IConversationRepository repository,
        IAiProvider aiProvider,
        IAiStreamingProvider aiStreamingProvider,
        IChatStreamNotifier notifier,
        IConversationTitleGenerator titleGenerator)
    {
        _repository = repository;
        _aiProvider = aiProvider;
        _aiStreamingProvider = aiStreamingProvider;
        _notifier = notifier;
        _titleGenerator = titleGenerator;
    }

    public async Task<string> HandleAsync(SendMessageCommand command, CancellationToken ct = default)
    {
        var conversation = await _repository.GetAsync(command.ConversationId, ct);

        if (conversation is null)
        {
            throw new Exception("Conversation not found");
        }
        conversation.AddMessage(command.Message, Domain.ValueObject.MessageRole.User); // insert user message
        //تولید خودکار عنوان Conversation
        if (conversation.Messages.Count == 1)
        {    
            var title =
                await _titleGenerator.GenerateTitleAsync(command.Message);
            if (!string.IsNullOrWhiteSpace(title))
            {
                conversation.Rename(title);
            }
        }

        await _repository.SaveChangesAsync();

        var messages =
            conversation.Messages.TakeLast(20)
                .Select(x =>
                    new MessageDto
                    {
                        Role = x.Role.ToString().ToLower(),
                        Content = x.Content,
                        Id = x.Id,  
                        CreatedAt = x.CreatedAt,
                    })
                .ToList();

        // var answer = await _aiProvider.AskAsync(messages);
        //conversation.AddMessage(answer, Domain.ValueObject.MessageRole.Assistant); // insert answer from AI provider 

        var answerBuilder = new StringBuilder();
        try
        {
            await _aiStreamingProvider.StreamAsync(messages,
                 async chunk =>
                 {
                     answerBuilder.Append(chunk);

                     await _notifier.SendChunkAsync(
                         conversation.Id,
                         chunk);
                 }, ct);
        }
        finally //این باعث می‌شود حتی اگر Ollama خطا بدهد، frontend برای همیشه قفل نماند.

        {
            // بعد از اتمام پاسخ از سمت مدل هوش مصنوعی، از طریق سیگنال ار به فرانت اعلام میکنیم
            await _notifier.CompleteAsync(command.ConversationId);
        }
        var answer = answerBuilder.ToString();
        conversation.AddMessage(answer, Domain.ValueObject.MessageRole.Assistant);
        await _repository.SaveChangesAsync();
        return answer;
    }
}