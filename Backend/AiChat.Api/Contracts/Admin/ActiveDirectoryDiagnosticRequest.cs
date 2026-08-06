namespace AiChat.Api.Contracts.Admin;

public sealed class ActiveDirectoryDiagnosticRequest
{
    public ActiveDirectoryDiagnosticType Type { get; set; }

    /// <summary>
    /// اختیاری؛ اگر ارسال نشود از تنظیمات ذخیره‌شده استفاده می‌شود.
    /// </summary>
    public string? Server { get; set; }

    public string? Domain { get; set; }
}
