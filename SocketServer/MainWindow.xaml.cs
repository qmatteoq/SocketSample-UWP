using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace SocketServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket socketChannel;
        private Socket socket;
        private const int BufferSize = 1024;
        private byte[] readBuffer;
        private ObservableCollection<string> messages;

        public MainWindow()
        {
            InitializeComponent();
            messages = new ObservableCollection<string>();
            Messages.ItemsSource = messages;
        }

        private void OnCreateSocketClicked(object sender, RoutedEventArgs e)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Loopback;
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 1983);
            socket.Bind(endPoint);
            socket.Listen(10);
            socket.BeginAccept(AcceptCallback, socket);
        }

        private async void ReadCallback(IAsyncResult ar)
        {
            int received = socketChannel.EndReceive(ar);
            if (received > 0)
            {
                string result = Encoding.ASCII.GetString(readBuffer, 0, received);
                await Dispatcher.InvokeAsync(() =>
                {
                    messages.Add(result);
                });

            }
            socketChannel.BeginReceive(readBuffer, 0, BufferSize, 0, ReadCallback, null);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            socketChannel = socket.EndAccept(ar);
            readBuffer = new byte[BufferSize];
            socketChannel.BeginReceive(readBuffer, 0, BufferSize, 0, ReadCallback, null);
        }

        private void OnSendTextClicked(object sender, RoutedEventArgs e)
        {
            if (socketChannel != null)
            {
                string message = Message.Text;
                byte[] bytes = Encoding.ASCII.GetBytes(message);
                socketChannel.Send(bytes);
            }
        }
    }
}
