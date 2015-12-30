using System;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Notifications;

namespace SocketTask
{
    public sealed class ReceiveMessagesTask: IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            var details = taskInstance.TriggerDetails as SocketActivityTriggerDetails;
            var socketInformation = details.SocketInformation;

            if (details.Reason == SocketActivityTriggerReason.SocketActivity)
            {
                StreamSocket socket = socketInformation.StreamSocket;
                DataReader reader = new DataReader(socket.InputStream);
                reader.InputStreamOptions = InputStreamOptions.Partial;
                await reader.LoadAsync(250);
                var dataString = reader.ReadString(reader.UnconsumedBufferLength);
                ShowToast(dataString);
                socket.TransferOwnership(socketInformation.Id);
            }

            deferral.Complete();
        }


        public void ShowToast(string text)
        {
            string xml =
                $@"
    <toast activationType='foreground' launch='args'>
        <visual>
            <binding template='ToastGeneric'>
                <text>Message from the socket</text>
                <text>{text}</text>
            </binding>
        </visual>
    </toast>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            ToastNotification notification = new ToastNotification(doc);
            ToastNotifier notifier = ToastNotificationManager.CreateToastNotifier();
            notifier.Show(notification);
        }
    }
}
