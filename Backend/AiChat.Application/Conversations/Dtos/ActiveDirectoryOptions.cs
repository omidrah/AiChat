namespace AiChat.Application.Conversations.Dtos
{
    public sealed class ActiveDirectoryOptions
    {
        public string Domain { get; set; } = "";
        public string? Server { get; set; }
        public string? Container { get; set; }
    }

}
