namespace AiChat.Application.Common.Ai;

public class OllamaModelInfo
{
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; } // حجم مدل به صورت خوانا (مثلا 4.7 GB)
    public string Format { get; set; } = string.Empty;
    public string Family { get; set; } = string.Empty;
    public string ParameterSize { get; set; } = string.Empty;
}
