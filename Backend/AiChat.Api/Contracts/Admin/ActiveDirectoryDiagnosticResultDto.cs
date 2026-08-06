namespace AiChat.Api.Contracts.Admin;

public sealed class ActiveDirectoryDiagnosticResultDto
{
    public bool Success { get; set; }

    public string TestName { get; set; } = string.Empty;

    public string Command { get; set; } = string.Empty;

    public string Output { get; set; } = string.Empty;

    public string? Error { get; set; }

    public long DurationMs { get; set; }

    public DateTimeOffset ExecutedAt { get; set; } = DateTimeOffset.UtcNow;
}