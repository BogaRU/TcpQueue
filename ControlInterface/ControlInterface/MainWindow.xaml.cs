using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ControlInterface
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private NetworkStream stream;

        public MainWindow()
        {
            InitializeComponent();
            InitializeConnection();
            toggleButton.Checked += ToggleButton_Checked;
            toggleButton.Unchecked += ToggleButton_Unchecked;
        }

        private void InitializeConnection()
        {
            int maxRetries = 5;
            int delayBetweenRetries = 2000;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    client = new TcpClient("127.0.0.1", 5005);
                    stream = client.GetStream();
                    MessageBox.Show("Подключение успешно установлено!");
                    return;
                }
                catch (SocketException)
                {
                    if (attempt < maxRetries - 1)
                    {
                        MessageBox.Show($"Не удалось подключиться, попытка {attempt + 1}. Повтор через 2 секунды...");
                        Thread.Sleep(delayBetweenRetries);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось подключиться к серверу. Пожалуйста, запустите QueueDisplay и попробуйте снова.");
                        stream?.Close();
                        client?.Close();
                        Close();
                    }
                }
            }
        }


        private void Reconnect()
        {
            try
            {
                client.Close();
                client = new TcpClient("127.0.0.1", 5000);
                stream = client.GetStream();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
            }
        }


        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            statusIndicator.Fill = Brushes.Green;
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            statusIndicator.Fill = Brushes.Yellow;
        }

        private void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            string command = toggleButton.IsChecked == true ? "AddGood" : "AddDefect";
            SendMessage(command);
        }

        private void PusherButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage("Remove");
        }

        private void SendMessage(string message)
        {
            if (stream == null) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
            catch (IOException ex)
            {
                MessageBox.Show("Ошибка при отправке данных: " + ex.Message);
                Reconnect();
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            stream?.Close();
            client?.Close();
        }
    }
}
