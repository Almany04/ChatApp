using ChatApp.Endpoint.Models;
using ChatApp.Endpoint.Services;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Endpoint.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ChatHistoryService _chatHistory;

        public ChatHub(ChatHistoryService chatHistory)
        {
            _chatHistory = chatHistory;
        }

        // Ezt a metódust fogja hívni a kliens, amikor üzeneteket küld
        public async Task SendMessage(string user, string message)
        {
            var chatMessage = new ChatMessage
            {
                UserName = user,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            // 1. Mentsd a memóriába
            _chatHistory.AddMessage(chatMessage);

            // 2. Küldd ki minden kliensnek
            await Clients.All.SendAsync("ReceiveMessage", chatMessage);
        }

        // Amikor egy új kliens csatlakozik, lefut ez a metódus
        public override async Task OnConnectedAsync()
        {
            // Küldd el a teljes chat előzményt csak a csatlakozó kliensnek
            var history = _chatHistory.GetAllMessages();
            await Clients.Caller.SendAsync("ReceiveHistory", history);
            await base.OnConnectedAsync();
        }
    }
}
