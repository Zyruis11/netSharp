using System;
using System.Net;
using System.Net.Sockets;

namespace Client.Objects.Session
{
    internal class Session : IDisposable
    {
        private SessionWorker _sessionWorker;
        public int HelloInterval;
        public bool IsDisposed;
        public int LastHeard;
        public TcpClient TcpClient;

        public void Dispose()
        {
            Close();
            IsDisposed = true;
        }

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

        public void Close()
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


    }
}