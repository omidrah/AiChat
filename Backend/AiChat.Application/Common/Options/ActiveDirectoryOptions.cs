namespace AiChat.Application.Common.Options;

public sealed class ActiveDirectoryOptions
{
    public bool Enabled { get; set; }

    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// مثال: DC=omid,DC=ir
    /// </summary>
    public string Container { get; set; } = string.Empty;

    /// <summary>
    /// DC اصلی؛ می‌تواند FQDN یا IP باشد.
    /// </summary>
    public string Server { get; set; } = string.Empty;

    public List<string> Servers { get; set; } = [];

    public bool UseSsl { get; set; }
}