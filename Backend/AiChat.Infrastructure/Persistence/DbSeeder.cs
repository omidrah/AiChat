using AiChat.Application.Abstractions;
using AiChat.Application.Authentications.Dtos;
using AiChat.Domain.Entities;

namespace AiChat.Infrastructure.Persistence
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ChatDbContext db, IPasswordHasher hasher , string authenticationMode)
        {
            if (db.Users.Any())
                return;

            if(!Enum.TryParse<AuthenticationProviderEnum>(authenticationMode, true, out var provider))
            {
                provider = AuthenticationProviderEnum.Local;
            }
            var user = User.CreateUser
            (                
                "admin",
                hasher.Hash("123456"),
                "Administrator",
                null,
                provider.ToString()
            );

            db.Users.Add(user);

            await db.SaveChangesAsync();
        }
    }
}
