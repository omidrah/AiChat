using AiChat.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace AiChat.Api.Hubs;
[Authorize]
public class ChatHub : Hub
{
    private readonly IConversationRepository _conversations;

    public ChatHub(IConversationRepository conversations)
    {
        _conversations = conversations;
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"SignalR connected: {Context.ConnectionId}");
        Console.WriteLine(Context.UserIdentifier);

        Console.WriteLine(Context.User?.Identity?.Name);

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"SignalR disconnected: {Context.ConnectionId}");
        Console.WriteLine(exception?.Message);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConversation(Guid conversationId)
    {
        // Console.WriteLine($"JoinConversation called. Connection: {Context.ConnectionId}, Conversation: {conversationId}");
        var userIdClaim =
               Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               Context.User?.FindFirst("sub")?.Value ??
               Context.User?.FindFirst("userId")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new HubException("Unauthorized");

        var conversation = await _conversations.GetConversationForUserAsync(
            conversationId,
            userId,
            Context.ConnectionAborted);

        if (conversation is null)
            throw new HubException("Conversation not found");

        await Groups.AddToGroupAsync(Context.ConnectionId, GetConversationGroupName(conversationId, userId));

    }
    private static string GetConversationGroupName(Guid conversationId, Guid userId)
    {
        return $"conversation:{conversationId}:user:{userId}";
    }
}