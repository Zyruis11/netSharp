using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Networking;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using netSharp.Core.Data;
using netSharp.Mobile.Events;
using Buffer = Windows.Storage.Streams.Buffer;

namespace netSharp.Mobile.Connectivity
{
    public class MobileSession
    {
        private StreamSocket clientSocket;
        public bool Connected { get; set; }
        private bool Closing { get; set; }

        public MobileSession(HostName hostname, string port, string localGuid)
        {
            ConnectAsync(hostname, port);
        }

        public void Dispose()
        {
            clientSocket.Dispose();
            clientSocket = null;
            Connected = false;
        }

        public event EventHandler<MobileEvents> SessionDataReceieved;

        // Event Handler-Trigger Binding
        protected virtual void EventInvocationWrapper(MobileEvents mobileEvents,
            EventHandler<MobileEvents> eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, mobileEvents);
            }
        }

        public void SessionDataReceivedTrigger(DataStream dataStream, MobileSession mobileSession)
        {
            EventInvocationWrapper(new MobileEvents(dataStream, this), SessionDataReceieved);
        }

        private async void ConnectAsync(HostName serverHostName, string serverPort)
        {
            await clientSocket.ConnectAsync(serverHostName, serverPort);
            Connected = true;
            StreamReaderAsync();
        }

        private async void StreamReaderAsync()
        {
            while (Connected)
            {
                try
                {
                    var buffer = new byte[1024].AsBuffer();
                    await clientSocket.InputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.Partial);
                    var result = buffer.ToArray();
                }
                catch
                {
                    break;
                }
            }

            if (Closing)
            {
                Dispose();
            }
        }

        private async void StreamWriterAsync(DataStream dataStream)
        {
            if (!Connected)
            {
                throw new Exception("Mobile session not connected.");
            }

            try
            {
                var buffer = DataStreamFactory.GetStreamByteArray(dataStream).AsBuffer();
                await clientSocket.OutputStream.WriteAsync(buffer);
            } 
            catch (Exception exception)
            {
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }
            }
        }
    }
}