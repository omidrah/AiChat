using System;

namespace AvaloniaApplication1
{
    public class MessageDto
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public record SendMessageRequest(string Message);

}
