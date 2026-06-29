using AiChat.Api.Hubs;
using AiChat.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace AiChat.Api.Contracts
{
    public class SignalRChatNotifier: IChatStreamNotifier
    {
        private readonly IHubContext<ChatHub> _hub;

        public SignalRChatNotifier(IHubContext<ChatHub> hub)
        {
            _hub = hub;
        }

        public async Task SendChunkAsync(Guid conversationId, Guid userId, string chunk)
        {
            Console.WriteLine($"Sending chunk = {chunk}");
            await _hub.Clients
                 .Group($"conversation:{conversationId}:user:{userId}")
                .SendAsync("ReceiveToken", chunk);
        }
        public async Task CompleteAsync(Guid conversationId, Guid userId)
        {
            Console.WriteLine($"Message from Ai is completed = {conversationId}");

            await _hub.Clients
                .Group($"conversation:{conversationId}:user:{userId}")
                .SendAsync("ReceiveCompleted");
        }

    }
}
