using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AiChat.Api.Hubs;
[Authorize]
public class ChatHub : Hub
{
        public override async Task OnConnectedAsync()
        {
             Console.WriteLine($"SignalR connected: {Context.ConnectionId}");
              await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"SignalR disconnected: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

         public async Task JoinConversation(string conversationId)
        {
            // Console.WriteLine($"JoinConversation called. Connection: {Context.ConnectionId}, Conversation: {conversationId}");

            await Groups.AddToGroupAsync( Context.ConnectionId, conversationId);

            // Console.WriteLine($"Connection {Context.ConnectionId} joined group {conversationId}");
         }
}