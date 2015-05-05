using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TcpIpServer
{
    internal class ServerListener : IDisposable
    {
        private Server _server;
        private IPAddress _listenAddress;
        private int _listenPort;
        private TcpListener _tcpListener;
        private Thread ServerListenerThread;
        public bool IsDisposed;
        public bool AcceptsNewClients;

        public ServerListener(Server server, IPAddress listenAddress, int listenPort)
        {
            _server = server;
            _listenAddress = listenAddress;
            _listenPort = listenPort;
        }

        public void StartListener()
        {
            ServerListenerThread = new Thread(Start);
            ServerListenerThread.Start();
            IsDisposed = false;
            AcceptsNewClients = true;
        }

        public void StopListener()
        {
            IsDisposed = true;
            AcceptsNewClients = false;
            TcpInternalConnection();
        }

        // Since the Listener loop blocks waiting for new connections, we will connect
        // to ourselves and then disconnect. Since this will only run when we are not
        // accepting clients there is no risk of creating irrelevant client objects.
        private static void TcpInternalConnection()
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(serverEndPoint);
            tcpClient.Close();
        }

        private void Start()
        {
            _tcpListener = new TcpListener(_listenAddress, _listenPort);
            _tcpListener.Start();

            while (!IsDisposed)
            {
                var tcpClient = _tcpListener.AcceptTcpClient();
             
                if (AcceptsNewClients)
                {
                    var client = new Client(_server, tcpClient);
                    client.Alive = true;
                    client.LastHeard = 0;
                    
                    bool addSuccess = _server.AddClient(client);

                    if (!addSuccess)
                    {
                        client.Dispose();
                    }
                }
            }
            _tcpListener.Stop();
        }

        public void Dispose()
        {
            _tcpListener.Stop();
            IsDisposed = true;
        }
    }
}