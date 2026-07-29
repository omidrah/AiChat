namespace AiChat.Application.Common.Ai;
public class OllamaServerDetails
{
    public string Version { get; set; } = "Unknown";
    public bool IsConnected { get; set; }
    public List<OllamaModelInfo> Models { get; set; } = new();
}
