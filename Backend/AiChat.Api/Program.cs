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


var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
           .AllowCredentials(); //disable in front and back
    });
});

var app = builder.Build();

app.UseCors("AngularClient");

//if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

//if (app.Environment.IsDevelopment())
//{
//    app.UseHttpsRedirection();
//}

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");

app.Run();
