using AiChat.Api.Contracts;
using AiChat.Api.Hubs;
using AiChat.Application.Abstractions;
using AiChat.Application.Conversations.Commands.CreateMessage;
using AiChat.Infrastructure.AI;
using AiChat.Infrastructure.Persistence;
using AiChat.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddHttpClient<IAiProvider, OllamaProvider>((client) => {
    client.Timeout = Timeout.InfiniteTimeSpan;
});
builder.Services.AddHttpClient<IAiStreamingProvider, OllamaStreamingProvider>((client)=> { 
    client.Timeout = Timeout.InfiniteTimeSpan;
});

builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection("Ollama"));

builder.Services.AddDbContext<ChatDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration.GetConnectionString("SqliteConnection"));

    //options.EnableSensitiveDataLogging();

    //options.LogTo(Console.WriteLine);
});
builder.Services.AddScoped<IChatStreamNotifier, SignalRChatNotifier>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IConversationTitleGenerator,OllamaConversationTitleGenerator>();
builder.Services.AddScoped<SendMessageHandler>();

var app = builder.Build();

app.MapHub<ChatHub>("/hubs/chat");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
