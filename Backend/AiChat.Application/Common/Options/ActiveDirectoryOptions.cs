namespace AiChat.Application.Common.Options;

public sealed class ActiveDirectoryOptions
{
    public bool Enabled { get; set; }

    //Domain = نام دامین/realm (مثل omid.ir یا OMID)

    public string Domain { get; set; } = string.Empty;

    /// <summary>
    // Base DN یا کانتینر (اختیاری)
    /// </summary>
    public string Container { get; set; } = string.Empty;

    /// <summary>
    // DC اصلی (FQDN یا IP) - اولویت اول اتصال
    /// </summary>
    public string PrimaryServer { get; set; } = string.Empty;

    /// <summary>
    // لیست DCهای جایگزین - اولویت‌های بعدی
    /// </summary>
    public List<string> FallbackServers { get; set; } = [];

    public bool UseSsl { get; set; }
}