using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using netSharp.RequestHandling;
using netSharp.TCP.Features;
using Timer = System.Timers.Timer;

namespace netSharp.TCP
{
    public class Server : IDisposable
    {
        private readonly IPAddress _listenAddress;
        private readonly int _listenPort;
        private readonly Timer _serverTimer;
        private readonly TcpListener _tcpListener;
        private readonly Guid serverGuid;
        private int _clientFactoryExceptionCount;
        private Thread _clientFactoryThread;

        public Server(string serverBindAddr, int serverBindPort, int maxClientRequests, int maxClientCount)
        {
            _listenAddress = IPAddress.Parse(serverBindAddr);
            _listenPort = serverBindPort;

            _tcpListener = new TcpListener(_listenAddress, _listenPort);
            SessionList = new List<Session>();
            RequestBroker = new RequestBroker(this);
            _serverTimer = new Timer(1000);
            _serverTimer.Elapsed += ServerTimerTick;
            _serverTimer.Enabled = true;

            MaxClientRequests = maxClientRequests;
            MaxClientCount = maxClientCount;

            _listenAddress = IPAddress.Parse(serverBindAddr);
            _listenPort = serverBindPort;

            serverGuid = Guid.NewGuid();

            StartClientSessionFactory();
        }

        public List<Session> SessionList { get; set; }
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

            foreach (Session session in SessionList)
            {
                if (session.RemoteEndpointGuid == Guid.Empty)
                {
                    session.SendString("GUID_GET");
                }
            }
        }

        /// <summary>
        ///     Iterates through the client list, any clients meeting conditions are added to a disposal list and then disposed.
        ///     Method is thread-safe.
        /// </summary>
        private void ClientGarbageCollector()
        {
            Heartbeat.Pulse(SessionList);
        }

        private void SessionFactory()
        {
            Thread.Sleep(1000);

            _tcpListener.Start();

            while (!IsDisposed)
            {
                try
                {
                    var tcpClient = _tcpListener.AcceptTcpClient();
                    var clientObject = new Session(0, null, serverGuid, tcpClient);
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
            lock (SessionList)
            {
                foreach (var client in SessionList)
                {
                    client.SendString(broadcastString);
                }
            }
        }

        public void StartClientSessionFactory()
        {
            _clientFactoryThread = new Thread(SessionFactory);
            _clientFactoryThread.Start();
        }

        public bool AddClient(Session client)
        {
            lock (SessionList)
            {
                if (SessionList.Count >= MaxClientCount) return false;
                SessionList.Add(client);
                return true;
            }
        }

        public bool RemoveClient(Session client)
        {
            lock (SessionList)
            {
                if (!SessionList.Contains(client)) return false;
                SessionList.Remove(client);
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