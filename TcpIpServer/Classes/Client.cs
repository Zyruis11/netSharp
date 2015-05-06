using System;
using System.Net.Sockets;

namespace TcpIpServer.Classes
{
    internal class Client : IDisposable
    {
        private ClientWorker _clientWorker;
        public Server _server;
        public TcpClient _tcpClient;
        public string guid;
        public bool IsDisposed;
        public bool Alive;
        public int LastHeard;
        public string CurrentStatus;

        public Request clientRequest;

        public Client(Server server, TcpClient tcpClient)
        {
            _server = server;
            _tcpClient = tcpClient;
            guid = Convert.ToString(Guid.NewGuid()).Remove(5);
            _clientWorker = new ClientWorker(this);
        }

        public void Dispose()
        {
            _tcpClient.Close();
            IsDisposed = true;
        }
    }
}