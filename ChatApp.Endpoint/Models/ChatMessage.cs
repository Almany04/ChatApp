namespace ChatApp.Endpoint.Models
{
    public class ChatMessage
    {
        public string UserName { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
