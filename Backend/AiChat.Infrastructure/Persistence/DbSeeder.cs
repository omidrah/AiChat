using AiChat.Application.Abstractions;
using AiChat.Domain.Entities;

namespace AiChat.Infrastructure.Persistence
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ChatDbContext db, IPasswordHasher hasher)
        {
            if (db.Users.Any())
                return;

            var user = new User
            {
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
