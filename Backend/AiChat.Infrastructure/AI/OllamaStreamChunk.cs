namespace AiChat.Infrastructure.AI
{
    public class OllamaStreamChunk
    {
        public OllamaStreamMessage Message { get; set; }
            = new();

        public bool Done { get; set; }
    }
}