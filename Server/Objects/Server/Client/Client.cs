using System;
using System.Net.Sockets;

namespace Server.Objects.Server.Client
{
    internal class Client : IDisposable
    {
        private ClientWorker _clientWorker;
        public Request.Request clientRequest;
        public string CurrentStatus;
        public string Guid;
        public bool IsDisposed;
        public int LastHeard;
        public string RemoteEp;
        public Server Server;
        public TcpClient TcpClient;

        public Client(Server server, TcpClient tcpClient)
        {
            Server = server;
            TcpClient = tcpClient;
            LastHeard = 0;
            RemoteEp = TcpClient.Client.RemoteEndPoint.ToString();
        }

        public void Dispose()
        {
            Close();
            IsDisposed = true;
        }

        public void Close()
        {
            TcpClient.Close();

            if (!TcpClient.Connected)
            {
                Console.WriteLine("Connection Closed");
            }
        }

        public void StartWorker()
        {
            _clientWorker = new ClientWorker(this);
        }

        public void SendString(string command)
        {
            _clientWorker.Responder(command);
        }
    }
}