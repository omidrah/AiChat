using AiChat.Api.Contracts;
using AiChat.Api.Contracts.Admin;
using AiChat.Api.Hubs;
using AiChat.Api.Services;
using AiChat.Application;
using AiChat.Application.Abstractions;
using AiChat.Application.Common.Auth;
using AiChat.Application.Common.Options;
using AiChat.Application.Conversations.Dtos;
using AiChat.Application.Users.Commands.CreateUser;
using AiChat.Application.Users.Commands.DeleteUser;
using AiChat.Application.Users.Commands.UpdateUser;
using AiChat.Application.Users.Queries.GetAllUsers;
using AiChat.Infrastructure.AI;
using AiChat.Infrastructure.Persistence;
using AiChat.Infrastructure.Persistence.Repositories;
using AiChat.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "appsettings.Runtime.json",
    optional: true,
    reloadOnChange: true);


var authMode = builder.Configuration["Authentication:Mode"] ?? "Local";
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Authentication:Jwt"));
builder.Services.Configure<ActiveDirectoryOptions>(builder.Configuration.GetSection("Authentication:ActiveDirectory"));

builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection("Ollama"));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IActiveDirectoryAuthService, ActiveDirectoryAuthService>();
builder.Services.AddSingleton<IActiveDirectorySettingsService, ActiveDirectorySettingsService>();
builder.Services.AddScoped<IActiveDirectoryDiagnosticService, ActiveDirectoryDiagnosticService>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration
                    .GetSection("Authentication:Jwt")
                    .Get<JwtOptions>();

        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions!.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Key)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Console.WriteLine($"Path = {context.Request.Path}");
                //  Console.WriteLine($"Query = {context.Request.QueryString}");

                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });
  //  .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserPolicy",
        policy =>
        {
            policy
                .AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme
                    //,NegotiateDefaults.AuthenticationScheme
                    )
                .RequireAuthenticatedUser();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddMemoryCache(); //use in ai HealthCheck then Register In-Memory Caching in DI

builder.Services.AddHttpClient<IAiProvider, OllamaProvider>((client) => {
    client.Timeout = Timeout.InfiniteTimeSpan;
});
builder.Services.AddHttpClient<IAiStreamingProvider, OllamaStreamingProvider>((client)=> { 
    client.Timeout = Timeout.InfiniteTimeSpan;
});

//use ollamClient in AiHealthCheck
builder.Services.AddHttpClient("OllamaClient", (sp, client) =>
{
    var ollamaOptions = sp.GetRequiredService<IOptions<OllamaOptions>>().Value;
    client.BaseAddress = new Uri(ollamaOptions.BaseUrl);
});

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
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserResolver, UserResolver>();
builder.Services.AddSingleton<IChatCancellationTracker, ChatCancellationTracker>();
builder.Services.AddScoped<IAiHealthService, AiHealthService>();    

// Add Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
//

builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<UpdateUserHandler>();
builder.Services.AddScoped<DeleteUserHandler>();
builder.Services.AddScoped<GetUsersHandler>();

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
//run seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    await DbSeeder.SeedAsync(db, hasher, authMode);
}


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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat").RequireAuthorization();
app.UseExceptionHandler();

app.Run();
