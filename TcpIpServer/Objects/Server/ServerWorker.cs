using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server.Objects.Server
{
    internal class ServerWorker : IDisposable
    {
        private readonly IPAddress _listenAddress;
        private readonly int _listenPort;
        private readonly Server _server;
        private TcpListener _tcpListener;
        private bool _acceptExternalClients;
        public bool IsDisposed;
        private Thread _serverListenerThread;

        public void Dispose()
        {
            StopListener();
            IsDisposed = true;
        }

        public ServerWorker(Server server, IPAddress listenAddress, int listenPort)
        {
            _server = server;
            _listenAddress = listenAddress;
            _listenPort = listenPort;
        }

        public void StartListener()
        {
            _acceptExternalClients = true;
            _serverListenerThread = new Thread(NewClientAcceptLoop);
            _serverListenerThread.Start();
            IsDisposed = false;
        }

        public void StopListener()
        {
            _acceptExternalClients = false;
            InternalTcpConnection();
            IsDisposed = true;
        }

        // Since the server is blocking in AcceptTcpClient we will connect to ourselves internally to allow the Client Accept Loop to be escaped.
        public void InternalTcpConnection()
        {
            var internalEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
            var _internalTcpClient = new TcpClient();
            _internalTcpClient.Connect(internalEndpoint);
        }

        private void NewClientAcceptLoop()
        {
            try
            {
                _tcpListener = new TcpListener(_listenAddress, _listenPort);
                _tcpListener.Start();
                Console.Write("Entering Client Accept Loop\n");
                Console.Write("Enter a command :");
                while (!IsDisposed)
                {
                    var tcpClient = _tcpListener.AcceptTcpClient();
                    if (_acceptExternalClients)
                    {
                        var client = new Client.Client(_server, tcpClient);

                        var addSuccess = _server.AddClient(client);

                        if (!addSuccess)
                            // If we cannot add the client, dispose the object. The dispose method on the client will automatically close the conenction.
                        {
                            client.Dispose();
                        }
                        else // If we can add the client, start the worker thread.
                        {
                            client.StartWorker();
                            Console.Beep();
                        }
                    }
                    else
                    {
                        tcpClient.Close();
                        Console.Write("Client Accept Loop Stopped.\n");
                    }
                }
            }
            catch (Exception exception)
            {
                Console.Write("\n\n{0}\n\n", "Server Error :" + exception.Message);
            }
            finally
            {
                _tcpListener.Stop();
            }
        }
    }
}