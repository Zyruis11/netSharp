using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using netSharp.Core.Data;
using netSharp.Core.Interfaces;
using netSharp.Server.Events;

namespace netSharp.Server.Connectivity
{
    public class Session : IDisposable, ISession
    {
        private readonly TcpClient _tcpClient;
        private CancellationToken _asyncCancellationToken;
        private NetworkStream _networkStream;
        private CancellationTokenSource ctSource;

        public Session(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            Initialize();
        }

        public Session(IPEndPoint ipEndPoint)
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(ipEndPoint);
            Initialize();
        }

        public bool IsDisposed { get; set; }
        public string RemoteEndpointGuid { get; set; }
        public string LocalEndpointGuid { get; set; }
        public double IdleTime { get; set; }
        public double MaxIdleTime { get; set; }

        public void Dispose()
        {
            ctSource.Cancel();
            IsDisposed = true;
        }

        public async void ReadDataAsync()
        {
            _networkStream = _tcpClient.GetStream();

            while (!IsDisposed)
            {
                var protocolInfoBuffer = new byte[10];
                var initialBytesRead = 0;

                try
                {
                    initialBytesRead =
                        await
                            _networkStream.ReadAsync(protocolInfoBuffer, 0, protocolInfoBuffer.Length,
                                _asyncCancellationToken);
                }
                catch
                {
                    break;
                }

                if (_asyncCancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (initialBytesRead == 0)
                {
                    continue;
                }

                var dataStream = DataStreamFactory.InitializeStreamObject(protocolInfoBuffer);

                var payloadBytesRead = 0;
                var payloadBytesRemaining = dataStream.PayloadLength;
                dataStream.PayloadByteArray = new byte[dataStream.PayloadLength];

                while (payloadBytesRead < dataStream.PayloadLength)
                {
                    try
                    {
                        payloadBytesRead +=
                            await
                                _networkStream.ReadAsync(
                                    dataStream.PayloadByteArray, 0,
                                    payloadBytesRemaining, _asyncCancellationToken);
                    }
                    catch
                    {
                        break;
                    }

                    if (payloadBytesRead == 0)
                    {
                        return;
                    }

                    payloadBytesRemaining -= payloadBytesRead;
                }

                SessionDataRecievedTrigger(dataStream);
            }

            if (_asyncCancellationToken.IsCancellationRequested)
            {
                _networkStream.Close();
                _tcpClient.Close();
            }
        }

        public async void SendDataAsync(DataStream dataStream)
        {
            var byteArray = DataStreamFactory.GetStreamByteArray(dataStream);
            await
                _networkStream.WriteAsync(byteArray, 0, byteArray.Length,
                    _asyncCancellationToken);
        }

        private void Initialize()
        {
            ctSource = new CancellationTokenSource();
            _asyncCancellationToken = ctSource.Token;
            ReadDataAsync();
        }

        public event EventHandler<ServerEvents> SessionDataRecieved;

        protected virtual void EventInvocationWrapper(ServerEvents serverEvents,
            EventHandler<ServerEvents> eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, serverEvents);
            }
        }

        public void SessionDataRecievedTrigger(DataStream dataStream)
        {
            EventInvocationWrapper(new ServerEvents(dataStream, this), SessionDataRecieved);
        }
    }
}