using ChatApp.WpfClient.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatApp.WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HubConnection _connection;
        public ObservableCollection<string> Messages { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Messages = new ObservableCollection<string>();
            this.DataContext = this;

            // SignalR kapcsolat beállítása
            _connection = new HubConnectionBuilder()
                // FIGYELEM: A portszámot (7XXX) cseréld le a sajátodra!
                .WithUrl("https://localhost:7013/chathub")
                .WithAutomaticReconnect()
                .Build();

            // Eseménykezelő a teljes chat előzmény fogadására
            _connection.On<IEnumerable<ChatMessage>>("ReceiveHistory", (history) =>
            {
                Dispatcher.Invoke(() =>
                {
                    Messages.Clear();
                    foreach (var msg in history)
                    {
                        Messages.Add($"[{msg.Timestamp:HH:mm}] {msg.UserName}: {msg.Message}");
                    }
                });
            });

            // Eseménykezelő egy új üzenet fogadására
            _connection.On<ChatMessage>("ReceiveMessage", (msg) =>
            {
                // Az UI frissítését mindig a Dispatcher-en keresztül végezzük
                Dispatcher.Invoke(() =>
                {
                    Messages.Add($"[{msg.Timestamp:HH:mm}] {msg.UserName}: {msg.Message}");
                });
            });

            // Aszinkron metódus hívása a konstruktorból a kapcsolat indításához
            _ = ConnectToServer();
        }

        private async Task ConnectToServer()
        {
            try
            {
                await _connection.StartAsync();
                MessageBox.Show("Connected to the chat server!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}");
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(UserNameTextBox.Text) || string.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                MessageBox.Show("Please enter your name and a message.");
                return;
            }

            try
            {
                // A "SendMessage" metódus hívása a szerveren
                await _connection.InvokeAsync("SendMessage", UserNameTextBox.Text, MessageTextBox.Text);
                MessageTextBox.Clear(); // Üzenetmező kiürítése küldés után
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}");
            }
        }
    }
}