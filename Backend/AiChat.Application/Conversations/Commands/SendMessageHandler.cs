namespace AiChat.Application.Conversations.Commands;

using AiChat.Application.Abstractions;
using AiChat.Application.Dtos;
using AiChat.Application.Interfaces;
using System.Text;
using System.Text.Json;

public class SendMessageHandler
{
    private readonly IConversationRepository _repository;
    private readonly IAiProvider _aiProvider;
    private readonly IAiStreamingProvider _aiStreamingProvider;
    public SendMessageHandler(
        IConversationRepository repository,
        IAiProvider aiProvider,
         IAiStreamingProvider aiStreamingProvider)
    {
        _repository = repository;
        _aiProvider = aiProvider;
        _aiStreamingProvider = aiStreamingProvider;
    }

    public async Task<string> HandleAsync(
        SendMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        var conversation =await _repository.GetAsync(
                command.ConversationId, cancellationToken);

        if (conversation is null)
        {
            throw new Exception(
                "Conversation not found");
        }
        conversation.AddMessage(command.Message, Domain.ValueObject.MessageRole.User); // insert user message

        await _repository.SaveChangesAsync();

        var messages =
            conversation.Messages
                .Select(x =>
                    new MessageDto
                    {
                        Role = x.Role.ToString().ToLower(),
                        Content = x.Content
                    })
                .ToList();

        // var answer = await _aiProvider.AskAsync(messages);
        //conversation.AddMessage(answer, Domain.ValueObject.MessageRole.Assistant); // insert answer from AI provider 
    
        var answerBuilder = new StringBuilder();
        await _aiStreamingProvider.StreamAsync(messages,
                 chunk =>
                 {
                     Console.Write(chunk);
                     answerBuilder.Append(chunk);
                     return Task.CompletedTask;
                 });
        var answer = answerBuilder.ToString();
        conversation.AddMessage(answer, Domain.ValueObject.MessageRole.Assistant);
        await _repository.SaveChangesAsync();
        return answer;
    }
}