using ChatApp.ConsoleClient.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.ConsoleClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Console Chat Client");
            Console.Write("Please enter your name: ");
            var userName = Console.ReadLine();

            // SignalR kapcsolat beállítása
            var connection = new HubConnectionBuilder()
                // FIGYELEM: A portszámot (7XXX) cseréld le a sajátodra!
                .WithUrl("https://localhost:7013/chathub")
                .Build();

            // Előzmények fogadása
            connection.On<IEnumerable<ChatMessage>>("ReceiveHistory", (history) =>
            {
                Console.Clear();
                Console.WriteLine("--- Chat History ---");
                foreach (var msg in history)
                {
                    Console.WriteLine($"[{msg.Timestamp:HH:mm}] {msg.UserName}: {msg.Message}");
                }
                Console.WriteLine("--- End of History ---");
                Console.Write("Enter your message: ");
            });

            // Új üzenet fogadása
            connection.On<ChatMessage>("ReceiveMessage", (msg) =>
            {
                Console.WriteLine($"\n[{msg.Timestamp:HH:mm}] {msg.UserName}: {msg.Message}");
                Console.Write("Enter your message: ");
            });

            try
            {
                await connection.StartAsync();
                Console.WriteLine("Connected! Type your message and press Enter to send.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
                Console.ReadKey();
                return;
            }

            // Üzenetek küldése egy ciklusban
            while (true)
            {
                var message = Console.ReadLine();
                if (!string.IsNullOrEmpty(message))
                {
                    try
                    {
                        await connection.InvokeAsync("SendMessage", userName, message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send message: {ex.Message}");
                    }
                }
            }
        }
    }
}
