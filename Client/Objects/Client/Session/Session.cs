using System;
using System.Net;
using System.Net.Sockets;

namespace Client.Objects.Client.Session
{
    internal class Session : IDisposable
    {
        public SessionWorker _sessionWorker;
        public int HelloInterval;
        public bool IsDisposed;
        public int LastHeard;
        public TcpClient TcpClient;
        public string Guid;

        public Session(string sessionEndpoint, string clientGuid)
        {
            var sessionEndpointParse = sessionEndpoint.Split(':');
            var endpointIpAddress = sessionEndpointParse[0];
            var endpointPort = Convert.ToInt32(sessionEndpointParse[1]);
            var serverEndPoint = new IPEndPoint(IPAddress.Parse(endpointIpAddress), endpointPort);
            Guid = clientGuid;
            LastHeard = 0;
            HelloInterval = 1; // First Hello interval is short to speed GUID exchange.
            Connect(serverEndPoint);
        }

        public void Dispose()
        {
            Close();
            IsDisposed = true;
        }

        private void Connect(IPEndPoint serverEndPoint)
        {
            TcpClient = new TcpClient();

            TcpClient.Connect(serverEndPoint);

            if (TcpClient.Connected)
            {
                _sessionWorker = new SessionWorker(this);
                Console.Write("\nConnected to Server");
            }
        }

        public void Close()
        {
            TcpClient.Close();

            if (!TcpClient.Connected)
            {
                Console.WriteLine("Connection Closed");
            }
        }
    }
}