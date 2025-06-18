using ChatApp.Endpoint.Models;
using System.Text.Json;


namespace ChatApp.Endpoint.Services
{
    public class ChatHistoryService
    {
        private readonly List<ChatMessage> _messages = new List<ChatMessage>();
        private readonly string _filePath = "chathistory.json"; // Fájl neve

        // Üzenet hozzáadása a memóriában lévő listához
        public void AddMessage(ChatMessage message)
        {
            _messages.Add(message);
        }

        // Visszaadja az összes üzenetet
        public IEnumerable<ChatMessage> GetAllMessages()
        {
            return _messages;
        }

        // Betölti az üzeneteket a JSON fájlból indításkor
        public void LoadFromFile()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var loadedMessages = JsonSerializer.Deserialize<List<ChatMessage>>(json);
                if (loadedMessages != null)
                {
                    _messages.AddRange(loadedMessages);
                }
            }
        }

        // Elmenti az üzeneteket a JSON fájlba (ezt hívja majd a Hangfire)
        public void SaveToFile()
        {
            var json = JsonSerializer.Serialize(_messages);
            File.WriteAllText(_filePath, json);
            Console.WriteLine("Chat history saved to file."); // Visszajelzés a szerver konzolján
        }
    }
}
