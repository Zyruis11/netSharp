using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using Library.Networking.RequestHandling;
using Timer = System.Timers.Timer;

namespace Library.Networking.TCP
{
    public class Server : IDisposable
    {
        private readonly IPAddress _listenAddress;
        private readonly int _listenPort;
        private readonly Timer _serverTimer;
        private readonly TcpListener _tcpListener;
        private int _clientFactoryExceptionCount;
        private Thread _clientFactoryThread;

        public Server(string serverBindAddr, int serverBindPort, int maxClientRequests, int maxClientCount)
        {
            _listenAddress = IPAddress.Parse(serverBindAddr);
            _listenPort = serverBindPort;

            _tcpListener = new TcpListener(_listenAddress, _listenPort);
            ClientSessionList = new List<ClientSession>();
            RequestBroker = new RequestBroker(this);
            _serverTimer = new Timer(1000);
            _serverTimer.Elapsed += ServerTimerTick;
            _serverTimer.Enabled = true;

            MaxClientRequests = maxClientRequests;
            MaxClientCount = maxClientCount;

            _listenAddress = IPAddress.Parse(serverBindAddr);
            _listenPort = serverBindPort;

            StartClientSessionFactory();
        }

        public List<ClientSession> ClientSessionList { get; set; }
        public bool IsDisposed { get; set; }
        public RequestBroker RequestBroker { get; set; }
        public int MaxClientCount { get; set; }
        public int MaxClientRequests { get; set; }

        public void Dispose()
        {
            _tcpListener.Stop();
            IsDisposed = true;
        }

        private void ServerTimerTick(object source, ElapsedEventArgs eea)
        {
            ClientGarbageCollector();
            //ClientRequestCollector();
        }

        /// <summary>
        ///     Iterates through the client list, any clients meeting conditions are added to a disposal list and then disposed.
        ///     Method is thread-safe.
        /// </summary>
        private void ClientGarbageCollector()
        {
            var clientsToDispose = new List<ClientSession>();

            lock (ClientSessionList)
            {
                foreach (var client in ClientSessionList)
                {
                    client.LastHeard += 1;

                    if (client.LastHeard >= 30)
                    {
                        clientsToDispose.Add(client);
                    }
                }

                foreach (var client in clientsToDispose)
                {
                    client.Dispose();
                    ClientSessionList.Remove(client);
                }
            }
        }

        private void ClientSessionFactory()
        {
            Thread.Sleep(1000);

            _tcpListener.Start();

            while (!IsDisposed)
            {
                try
                {
                    var tcpClient = _tcpListener.AcceptTcpClient();
                    var clientObject = new ClientSession(tcpClient);
                    var addSuccess = AddClient(clientObject);

                    if (!addSuccess)
                    {
                        clientObject.Dispose();
                    }
                }
                catch (Exception)
                {
                    _clientFactoryExceptionCount++;

                    if (_clientFactoryExceptionCount <= 3)
                    {
                        StartClientSessionFactory();
                    }
                    else
                    {
                        throw new Exception("Client Factory exceeded failure threshold.");
                    }
                }
            }
            _tcpListener.Stop();
        }

        public void ClientBroadcast(string broadcastString)
        {
            lock (ClientSessionList)
            {
                foreach (var client in ClientSessionList)
                {
                    client.SendString(broadcastString);
                }
            }
        }

        public void StartClientSessionFactory()
        {
            _clientFactoryThread = new Thread(ClientSessionFactory);
            _clientFactoryThread.Start();
        }

        public bool AddClient(ClientSession client)
        {
            lock (ClientSessionList)
            {
                if (ClientSessionList.Count >= MaxClientCount) return false;
                ClientSessionList.Add(client);
                return true;
            }
        }

        public bool RemoveClient(ClientSession client)
        {
            lock (ClientSessionList)
            {
                if (!ClientSessionList.Contains(client)) return false;
                ClientSessionList.Remove(client);
                return true;
            }
        }

        public bool AddClientRequest(string guid)
        {
            bool returnBool;
            lock (RequestBroker)
            {
                returnBool = RequestBroker.AddRequest(guid);
            }
            return returnBool;
        }

        public bool RemoveClientRequest(string guid)
        {
            bool returnBool;
            lock (RequestBroker)
            {
                returnBool = RequestBroker.RemoveRequest(guid);
            }
            return returnBool;
        }
    }
}