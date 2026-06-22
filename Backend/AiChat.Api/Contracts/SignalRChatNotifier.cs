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

        public async Task SendChunkAsync(Guid conversationId,string chunk)
        {
            Console.WriteLine($"Sending chunk = {chunk}");
            await _hub.Clients
                .Group(conversationId.ToString())
                .SendAsync("ReceiveToken", chunk);
        }
        public async Task CompleteAsync(Guid conversationId)
        {
            Console.WriteLine($"Message from Ai is completed = {conversationId}");

            await _hub.Clients
                .Group(conversationId.ToString())
                .SendAsync("ReceiveCompleted");
        }

    }
}
