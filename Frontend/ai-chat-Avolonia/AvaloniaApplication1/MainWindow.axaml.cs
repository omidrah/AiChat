using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.Net.Http;
using System;
using Avalonia.Interactivity;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AvaloniaApplication1
{
    public partial class MainWindow : Window
    {
        private HubConnection _hubConnection;
        private HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7117") };
        private Guid _currentConversationId;
        public ObservableCollection<MessageDto> Messages { get; } = new();
        private MessageDto? _streamingMessage;
        public MainWindow()
        {
            InitializeComponent();
            MessagesList.ItemsSource = Messages;
            DataContext = this;
            // در شروع، یک کانورسیشن جدید می‌سازیم یا لود می‌کنیم
            _ = InitializeChat();
        }
        private async Task InitializeChat()
        {
            try
            {
                // 1. ایجاد مکالمه جدید از طریق API
                var response = await _httpClient.PostAsync("/api/conversations", null);
                var idString = await response.Content.ReadAsStringAsync();
                _currentConversationId = Guid.Parse(idString.Replace("\"", ""));

                // 2. اتصال به SignalR
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl("https://localhost:7117/hubs/chat")
                    .WithAutomaticReconnect()
                    .Build();

                // گوش دادن به پیام‌های احتمالی از سمت سرور
                _hubConnection.On<string>(
                    "ReceiveChunk",
                    (chunk) =>
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            if (_streamingMessage == null)
                            {
                                _streamingMessage = new MessageDto
                                {
                                    Role = "Assistant",
                                    Content = "",
                                    CreatedAt = DateTime.Now
                                };

                                Messages.Add(_streamingMessage);
                            }

                            _streamingMessage.Content += chunk;

                            MessagesList.ItemsSource = null;
                            MessagesList.ItemsSource = Messages;
                        });
                    });

                await _hubConnection.StartAsync();

                // 3. جوین شدن به گروه این مکالمه در SignalR
                await _hubConnection.InvokeAsync("JoinConversation", _currentConversationId.ToString());

                StatusLabel.Text = $"Connected to: {_currentConversationId}";
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error: {ex.Message}";
            }
        }

        private async void OnSendClick(object sender, RoutedEventArgs e)
        {
            _streamingMessage = null;

            var text = InputBox.Text;
            if (string.IsNullOrWhiteSpace(text)) return;

            // افزودن پیام کاربر به لیست
            var userMsg = new MessageDto { Role = "User", Content = text, CreatedAt = DateTime.Now };
            Messages.Add(userMsg);
            InputBox.Text = "";

            try
            {
                // ارسال به API (طبق متد SendMessage در کنترلر شما)
                var response = await _httpClient.PostAsJsonAsync(
                    $"/api/conversations/{_currentConversationId}/messages",
                    new SendMessageRequest(text));
                response.EnsureSuccessStatusCode(); 
                //if (response.IsSuccessStatusCode)
                //{
                //    var answer = await response.Content.ReadAsStringAsync();
                //    Messages.Add(new MessageDto
                //    {
                //        Role = "Assistant",
                //        Content = answer,
                //        CreatedAt = DateTime.Now
                //    });
                //}
            }
            catch (Exception ex)
            {
                Messages.Add(new MessageDto { Role = "System", Content = "Error: " + ex.Message });
            }
        }
    }
}