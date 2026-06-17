using AiChat.Api.Contracts;
using AiChat.Api.Hubs;
using AiChat.Application;
using AiChat.Application.Abstractions;
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
builder.Services.AddApplicationHandler();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
             .WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
}); 

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AngularClient");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");

app.Run();
