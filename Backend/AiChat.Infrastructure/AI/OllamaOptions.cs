namespace AiChat.Infrastructure.AI
{
    public class OllamaOptions
    {
        public required string BaseUrl { get; set; }
        public required string Model { get; set; }
        public required string ReasoningEffort { get; set; }   //reasoning_effort="none", # استدلال خاموش؛ برای جواب کوتاه و سریع
        
        public required string MaxCompletionTokens { get; set; } //max_completion_tokens= حداکثر ۵۰ توکن جواب بده # , 50
        public string[] StopSequences { get; set; }    // stop=["\n\n", " وقتی به خط خالی یا شماره ۴ رسید، متوقف شو # ,[". ۴
    }
}
