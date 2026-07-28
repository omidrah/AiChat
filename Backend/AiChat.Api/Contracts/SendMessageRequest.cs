namespace AiChat.Api.Contracts
{
    public class SendMessageRequest
    {
        public string Message { get; set; } = "";
        public string Model { get; set; }
    }
}
