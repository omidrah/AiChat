using AiChat.Api.Hubs;
using AiChat.Application.Abstractions;
using AiChat.Application.Conversations.Commands;
using AiChat.Application.Interfaces;
using AiChat.Infrastructure.AI;
using AiChat.Infrastructure.Persistence;
using AiChat.Infrastructure.Persistence.Repositories;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle  
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddHttpClient<IAiProvider, OllamaProvider>();
builder.Services.AddHttpClient<IAiStreamingProvider, OllamaStreamingProvider>();

// Fix: Use Configure instead of Configuration.  
builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection("Ollama"));

builder.Services.AddDbContext<ChatDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration.GetConnectionString("SqliteConnection"));

    options.EnableSensitiveDataLogging();

    options.LogTo(Console.WriteLine);
});

builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<SendMessageHandler>();

var app = builder.Build();

app.MapHub<ChatHub>("/hubs/chat");
// Configure the HTTP request pipeline.  
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
