using System;
using System.Net.Sockets;

namespace TcpIpServer.Classes
{
    internal class Client : IDisposable
    {
        private ClientWorker _clientWorker;
        public Server Server;
        public TcpClient TcpClient;
        public string Guid;
        public bool IsDisposed;
        public int LastHeard;
        public string CurrentStatus;
        public string RemoteEp;

        public Request clientRequest;

        public Client(Server server, TcpClient tcpClient)
        {
            Server = server;
            TcpClient = tcpClient;
            Guid = Convert.ToString(System.Guid.NewGuid()).Remove(5);
            LastHeard = 0;
            RemoteEp = TcpClient.Client.RemoteEndPoint.ToString();
        }

        public void StartWorker()
        {
            _clientWorker = new ClientWorker(this);
        }

        public void SendString(string command)
        {
            _clientWorker.Responder(command);
        }

        public void Dispose()
        {
            TcpClient.Close();
            IsDisposed = true;
        }
    }
}