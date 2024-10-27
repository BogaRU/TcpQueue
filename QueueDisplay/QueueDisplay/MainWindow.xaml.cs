using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace QueueDisplay
{
    public partial class MainWindow : Window
    {
        private TcpListener listener;
        private List<string> queue = new List<string>();
        private const int MaxQueueSize = 5;

        public MainWindow()
        {
            InitializeComponent();
            StartServer();
        }

        private void StartServer()
        {
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 5005);
            listener.Start();
            MessageBox.Show("Сервер запущен и ожидает подключения...");
            Thread listenerThread = new Thread(ListenForCommands);
            listenerThread.Start();
        }


        private void ListenForCommands()
        {
            while (true)
            {
                try
                {
                    using (TcpClient client = listener.AcceptTcpClient())
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] buffer = new byte[256];
                        while (true)
                        {
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0) break;

                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            ProcessCommand(message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка приема команды: " + ex.Message);
                }
            }
        }

        private void ProcessCommand(string message)
        {
            if (message == "AddGood")
                AddToQueue("Good");
            else if (message == "AddDefect")
                AddToQueue("Defect");
            else if (message == "Remove")
                RemoveFromQueue();
        }


        private void AddToQueue(string type)
        {
            if (queue.Count >= MaxQueueSize) return;

            queue.Insert(0, type);
            UpdateQueueDisplay();
        }

        private void RemoveFromQueue()
        {
            if (queue.Count == 0) return;

            queue.RemoveAt(queue.Count - 1);
            UpdateQueueDisplay();
        }

        private void UpdateQueueDisplay()
        {
            Dispatcher.Invoke(() =>
            {
                queuePanel.Children.Clear();
                foreach (var item in queue)
                {
                    Ellipse product = new Ellipse
                    {
                        Width = 50,
                        Height = 50,
                        Fill = item == "Good" ? Brushes.Green : Brushes.Yellow,
                        Margin = new Thickness(5)
                    };
                    queuePanel.Children.Add(product);
                }
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listener.Stop();
            listener = null;
        }
    }
}
