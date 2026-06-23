namespace AiChat.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string UserName { get; set; } = "";

        public string PasswordHash { get; set; } = "";

        public string? DisplayName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<string> Roles { get; set; } = new();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    }

}
