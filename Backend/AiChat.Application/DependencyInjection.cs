using AiChat.Application.Authentications.Commands.Login;
using AiChat.Application.Authentications.Queries.GetCurrentUser;
using AiChat.Application.Conversations.Commands.CreateConversation;
using AiChat.Application.Conversations.Commands.CreateMessage;
using AiChat.Application.Conversations.Commands.DeleteConversaion;
using AiChat.Application.Conversations.Commands.RenameConversation;
using AiChat.Application.Conversations.Queries.GetConversationList;
using AiChat.Application.Conversations.Queries.GetConverstaions;
using Microsoft.Extensions.DependencyInjection;

namespace AiChat.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationHandler(this IServiceCollection services)
    {
        services.AddScoped<CreateConversationHandler>();
        services.AddScoped<DeleteConversationHandler>();
        services.AddScoped<GetConversationHandler>();
        services.AddScoped<GetConversationListHandler>();
        services.AddScoped<SendMessageHandler>();
        services.AddScoped<RenameConversationHandler>();
        services.AddScoped<GetCurrentUserQueryHandler>();
        services.AddScoped<LoginCommandHandler>();

        return services;
    }
}