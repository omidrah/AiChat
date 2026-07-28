namespace AiChat.Application.Conversations.Commands.CreateMessage;

using AiChat.Application.Abstractions;
using AiChat.Application.Conversations.Dtos;
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

        var conversation = await _repository.GetConversationForUserAsync(command.ConversationId, command.UserId, ct);

        if (conversation is null)
        {
            throw new Exception("Conversation not found");
        }

        conversation.AddMessage(command.Message, command.Model, Domain.ValueObject.MessageRole.User); // insert user message

        //تولید خودکار عنوان Conversation
        if (conversation.Messages.Count == 1)
        {    
            var title =
                await _titleGenerator.GenerateTitleAsync(command.Message, command.Model, ct);
            if (!string.IsNullOrWhiteSpace(title))
            {
                conversation.Rename(title);
            }
        }
        // ۱. بررسی اینکه قبل از ذخیره‌سازی ، عملیات کنسل نشده باشد
        ct.ThrowIfCancellationRequested();

        await _repository.SaveChangesAsync(ct);

        var messages =
            conversation.Messages.TakeLast(20)
                .Select(x =>
                    new MessageDto
                    {
                        Role = x.Role.ToString().ToLower(),
                        Content = x.Content,
                        Model = x.Model ?? command.Model, // استفاده از مدلِ پیام یا مدل کامند جاری
                        Id = x.Id,  
                        CreatedAt = x.CreatedAt,
                    })
                .ToList();

        var answerBuilder = new StringBuilder();
        try
        {
            await _aiStreamingProvider.StreamAsync(messages,command.Model,
                 async chunk =>
                 {
                     // بررسی در هر تکه استریم شده که کاربر کنسل نکرده باشد ...
                     ct.ThrowIfCancellationRequested();
                     answerBuilder.Append(chunk);
                     await _notifier.SendChunkAsync(conversation.Id, command.UserId, chunk);
                 }, ct);
        }
        catch (OperationCanceledException)
        {
            // در صورت لغو، به فرانت‌اند اطلاع می‌دهیم که استریم قطع شده است
            await _notifier.CompleteAsync(command.ConversationId, command.UserId);
            throw; // پرتاب مجدد استثنا برای متوقف شدن کامل متد
        }

        finally 
        {
            // بعد از اتمام پاسخ از سمت مدل هوش مصنوعی، از طریق سیگنال ار به فرانت اعلام میکنیم
            await _notifier.CompleteAsync(command.ConversationId, command.UserId);
        }

        // ۳. بررسی نهایی؛ اگر در ثانیه‌های آخر استریم لغو شده باشد، در دیتابیس ذخیره نشود
        ct.ThrowIfCancellationRequested();

        var answer = answerBuilder.ToString();
        conversation.AddMessage(answer, command.Model, Domain.ValueObject.MessageRole.Assistant);
        await _repository.SaveChangesAsync(ct);
        return answer;
    }
}