namespace AiChat.Application.Common.Ai;

using System;

public class AiHealthStatus
{
    public bool IsHealthy { get; set; }
    public string Message { get; set; }
    public DateTime CheckedAt { get; set; }
}

