using Microsoft.AspNetCore.SignalR;

namespace AiChat.Api.Hubs;

public class ChatHub : Hub
{
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine( $"Connected : {Context.ConnectionId}");

            await base.OnConnectedAsync();
        }

        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync( Context.ConnectionId, conversationId);
        }
}