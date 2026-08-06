namespace AiChat.Api.Contracts.Admin;

public sealed class ActiveDirectorySettingsDto
{
    public bool Enabled { get; set; }

    public string Domain { get; set; } = string.Empty;

    public string Container { get; set; } = string.Empty;

    public string Server { get; set; } = string.Empty;

    public List<string> Servers { get; set; } = [];

    public bool UseSsl { get; set; }
}
