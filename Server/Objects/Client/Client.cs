using System;
using System.Net.Sockets;

namespace Server.Objects.Client
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
        public Server.Server Server;
        public TcpClient TcpClient;

        public void Dispose()
        {
            Close();
            IsDisposed = true;
        }

        public Client(Server.Server server, TcpClient tcpClient)
        {
            Server = server;
            TcpClient = tcpClient;
            Guid = Convert.ToString(System.Guid.NewGuid()).Remove(5);
            LastHeard = 0;
            RemoteEp = TcpClient.Client.RemoteEndPoint.ToString();
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