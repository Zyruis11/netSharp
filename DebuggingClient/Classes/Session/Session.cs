using System;
using System.Net;
using System.Net.Sockets;

namespace DebuggingClient.Classes
{
    internal class Session
    {
        private SessionWorker _sessionWorker;
        public bool IsDisposed;
        public TcpClient TcpClient;
        public int LastHeard;
        public int HelloInterval;

        public Session(string sessionEndpoint)
        {
            var sessionEndpointParse = sessionEndpoint.Split(':');
            var endpointIpAddress = sessionEndpointParse[0];
            var endpointPort = Convert.ToInt32(sessionEndpointParse[1]);
            var serverEndPoint = new IPEndPoint(IPAddress.Parse(endpointIpAddress), endpointPort);
            LastHeard = 0;
            HelloInterval = 5;
            Connect(serverEndPoint);
        }

        private void Connect(IPEndPoint serverEndPoint)
        {
            TcpClient = new TcpClient();

            TcpClient.Connect(serverEndPoint);

            if (TcpClient.Connected)
            {
                Console.Write("!");
                _sessionWorker = new SessionWorker(this);
            }
        }

        private void Close()
        {
            TcpClient.Close();

            if (!TcpClient.Connected)
            {
                Console.WriteLine("Connection Closed");
            }
        }

        public void SendHello()
        {
            _sessionWorker.Sender("CLIENT_HELLO");
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}