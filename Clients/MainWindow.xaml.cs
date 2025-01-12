using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Clients
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected = false;
        private string _ipAddress;
        private int _port;

        public MainWindow(string IpAddress, int Port)
        {
            InitializeComponent();
            _ipAddress = IpAddress;
            _port = Port;
        }

        private async void ConnectToServer(string server, int port)
        {
            try
            {
                client = new TcpClient(server, port);
                stream = client.GetStream();
                isConnected = true;
                LogMessage("Connected to server.");
                await ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}");
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            try
            {
                byte[] data = new byte[256];
                while (isConnected)
                {
                    int bytes = await stream.ReadAsync(data, 0, data.Length);
                    if (bytes > 0)
                    {
                        string response = Encoding.ASCII.GetString(data, 0, bytes);
                        LogMessage($"Received: {response}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}");
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                ConnectToServer(_ipAddress, _port);
                return;
            }

            string message = MessageInput.Text;
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            try
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
                LogMessage($"Sent: {message}");
                MessageInput.Clear();
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}");
            }
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                MessageList.Items.Add($"{DateTime.Now:t} - {message}");
                MessageList.SelectedIndex = MessageList.Items.Count - 1; // Scroll to the last message
            });
        }
    }
}
