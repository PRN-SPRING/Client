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
        private const int TEXT_MESSAGE = 1;
        private const int FILE_MESSAGE = 2;

        public MainWindow(string IpAddress, int Port)
        {
            InitializeComponent();
            _ipAddress = IpAddress;
            _port = Port;
            Task.Run(() => ConnectToServer(_ipAddress, _port));
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
        private async Task SendMessageAsync(string message)
        {
            try
            {
                // Send message type (1 for text)
                await stream.WriteAsync(new byte[] { TEXT_MESSAGE }, 0, 1);

                // Send the actual message
                byte[] messageData = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(messageData, 0, messageData.Length);

                LogMessage($"Sent message: {message}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error sending message: {ex.Message}");
                isConnected = false;
            }
        }

        private async Task UploadFileAsync(string filePath)
        {
            if (!isConnected)
            {
                LogMessage("Not connected to the server.");
                return;
            }

            if (!System.IO.File.Exists(filePath))
            {
                LogMessage("File not found.");
                return;
            }

            try
            {
                // Send message type (2 for file)
                await stream.WriteAsync(new byte[] { FILE_MESSAGE }, 0, 1);

                string fileName = System.IO.Path.GetFileName(filePath);
                byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                byte[] fileData = System.IO.File.ReadAllBytes(filePath);

                // Send file name length (4 bytes)
                byte[] fileNameLength = BitConverter.GetBytes(fileNameBytes.Length);
                await stream.WriteAsync(fileNameLength, 0, fileNameLength.Length);

                // Send file name
                await stream.WriteAsync(fileNameBytes, 0, fileNameBytes.Length);

                // Send file size (4 bytes)
                byte[] fileSize = BitConverter.GetBytes(fileData.Length);
                await stream.WriteAsync(fileSize, 0, fileSize.Length);

                // Send file data in chunks
                int chunkSize = 4096;
                int position = 0;
                while (position < fileData.Length)
                {
                    int remaining = fileData.Length - position;
                    int currentChunkSize = Math.Min(chunkSize, remaining);
                    await stream.WriteAsync(fileData, position, currentChunkSize);
                    position += currentChunkSize;

                    Dispatcher.Invoke(() => {
                        if (MessageList.Items.Count > 0)
                            MessageList.Items[MessageList.Items.Count - 1] = $"{DateTime.Now:t} - Upload progress: {Math.Round((double)position / fileData.Length * 100)}%";
                        else
                            MessageList.Items.Add($"{DateTime.Now:t} - Upload progress: {Math.Round((double)position / fileData.Length * 100)}%");
                    });
                }

                LogMessage($"File '{fileName}' uploaded successfully.");
            }
            catch (Exception ex)
            {
                LogMessage($"Error while uploading file: {ex.Message}");
                isConnected = false;
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
                await SendMessageAsync(message);
                MessageInput.Clear();
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}");
            }
        }

        private async void UploadFile_Click(object sender, RoutedEventArgs e)
        {
            if (!isConnected)
            {
                LogMessage("Not connected to the server.");
                return;
            }

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string filepath = openFileDialog.FileName;
                await UploadFileAsync(filepath);
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
