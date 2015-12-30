using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SocketClient
{
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<string> messages;
        private const string TaskName = "SocketTask";
        private IBackgroundTaskRegistration task;
        private const string SocketId = "SampleSocket";
        private bool isConnected;
        private StreamSocket socket;

        public MainPage()
        {
            this.InitializeComponent();
            messages = new ObservableCollection<string>();
            Messages.ItemsSource = messages;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var registration = BackgroundTaskRegistration.AllTasks.FirstOrDefault(x => x.Value.Name == TaskName);
            if (registration.Value == null)
            {
                var socketTaskBuilder = new BackgroundTaskBuilder();
                socketTaskBuilder.Name = TaskName;
                socketTaskBuilder.TaskEntryPoint = "SocketTask.ReceiveMessagesTask";
                var trigger = new SocketActivityTrigger();
                socketTaskBuilder.SetTrigger(trigger);
                var status = await BackgroundExecutionManager.RequestAccessAsync();
                if (status != BackgroundAccessStatus.Denied)
                {
                    task = socketTaskBuilder.Register();
                }
            }
            else
            {
                task = registration.Value;
            }
        }

        private async void OnForegroundSocketClicked(object sender, RoutedEventArgs e)
        {
            socket = new StreamSocket();
            HostName host = new HostName("localhost");

            try
            {
                await socket.ConnectAsync(host, "1983");

                isConnected = true;
                while (isConnected)
                {
                    try
                    {
                        DataReader reader;

                        using (reader = new DataReader(socket.InputStream))
                        {
                            // Set the DataReader to only wait for available data (so that we don't have to know the data size)
                            reader.InputStreamOptions = InputStreamOptions.Partial;
                            // The encoding and byte order need to match the settings of the writer we previously used.
                            reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                            reader.ByteOrder = ByteOrder.LittleEndian;

                            // Send the contents of the writer to the backing stream. 
                            // Get the size of the buffer that has not been read.
                            await reader.LoadAsync(256);

                            // Keep reading until we consume the complete stream.
                            while (reader.UnconsumedBufferLength > 0)
                            {
                                string readString = reader.ReadString(reader.UnconsumedBufferLength);
                                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    messages.Add(readString);
                                });
                                Debug.WriteLine(readString);
                                await reader.LoadAsync(256);
                            }

                            reader.DetachStream();

                        }

                    }
                    catch (Exception exc)
                    {
                        MessageDialog dialog = new MessageDialog("Error reading the data");
                        await dialog.ShowAsync();
                    }
                }
            }
            catch (Exception exc)
            {
                MessageDialog dialog = new MessageDialog("Error connecting to the socket");
                await dialog.ShowAsync();
            }
        }

        private async void OnBackgroundSocketClicked(object sender, RoutedEventArgs e)
        {
            socket = new StreamSocket();
            HostName host = new HostName("localhost");

            await socket.ConnectAsync(host, "1983");
            socket.EnableTransferOwnership(task.TaskId, SocketActivityConnectedStandbyAction.Wake);
            socket.TransferOwnership(SocketId);
        }

        private async void OnCloseConnectionClicked(object sender, RoutedEventArgs e)
        {
            isConnected = false;
            await socket.CancelIOAsync();
            socket.Dispose();
        }

        private async void OnSendMessageClicked(object sender, RoutedEventArgs e)
        {
            DataWriter writer = new DataWriter(socket.OutputStream);
            writer.WriteString("This is a sample message");
            await writer.StoreAsync();
            await writer.FlushAsync();
        }
    }
}
