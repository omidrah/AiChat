namespace AiChat.Application.Conversations.Dtos;

public sealed record RenameConversationRequest(Guid conversationId, string conversationTitle);