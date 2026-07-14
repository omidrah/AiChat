using AiChat.Application.Abstractions;
using AiChat.Application.Common.Enums;
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
            var user = new User
            {
                AuthProvider = provider.ToString(),
                Id = Guid.NewGuid(),
                UserName = "admin",
                PasswordHash = hasher.Hash("123456"),
                DisplayName = "Administrator",
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);

            await db.SaveChangesAsync();
        }
    }
}
