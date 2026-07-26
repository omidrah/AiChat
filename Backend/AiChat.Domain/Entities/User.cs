namespace AiChat.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }

        public string PasswordHash { get; private set; } = string.Empty;

        public string UserName { get; private set; } = string.Empty;

        public string? ExternalId { get; private set; }

        public string? AuthProvider { get; private set; }

        public string? DisplayName { get; private set; }

        public bool IsActive { get; private set; } = true;

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public List<string> Roles { get; private set; } = new();

        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

        // Required by EF Core
        private User()
        {
        }

        private User(string userName, string passwordHash, string? externalId, string authProvider)
        {
            Id = Guid.NewGuid();
            UserName = userName;
            PasswordHash = passwordHash;
            DisplayName = "Mr." + userName;
            IsActive = true;
            AuthProvider = authProvider;
            ExternalId = externalId;
            CreatedAt = DateTime.UtcNow;
        }

        public static User CreateUser(
            string userName,
            string passwordHash,
            string? externalId,
            string authProvider)
        {
            return new User(userName, passwordHash, externalId, authProvider);
        }

        public void SetIsActive(bool isActive)
        {
            IsActive = isActive;
        }

        public void ChangePassword(string passwordHash)
        {
            PasswordHash = passwordHash;
        }

        public void SetDisplayName(string? displayName)
        {
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? UserName : displayName;
        }
    }
}