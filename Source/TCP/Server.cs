using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using netSharp.RequestHandling;
using netSharp.TCP.Events;
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
        private readonly Guid _serverGuid;
        private int _clientFactoryExceptionCount;
        private Thread _clientFactoryThread;

        public event EventHandler<EventDataArgs> ClientRemoved;
        public event EventHandler<EventDataArgs> ClientCreated;
        public event EventHandler<EventDataArgs> ListenerPaused;
        public event EventHandler<EventDataArgs> NewClientRequest;

        // Event Handler-Trigger Binding
        protected virtual void EventInvocationWrapper(EventDataArgs eventDataArgs,
            EventHandler<EventDataArgs> eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, eventDataArgs);
            }
        }

        public void ClientCreatedTrigger()
        {
            EventInvocationWrapper(new EventDataArgs(), ClientCreated);
        }

        public void ListenerPausedTrigger()
        {
            EventInvocationWrapper(new EventDataArgs(), ListenerPaused);
        }

        public void ClientRemovedTrigger()
        {
            EventInvocationWrapper(new EventDataArgs(), ClientRemoved);
        }

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

            _serverGuid = Guid.NewGuid();

            StartClientSessionFactory();
        }

        public List<Session> SessionList { get; set; }
        public bool IsDisposed { get; set; }
        public RequestBroker RequestBroker { get; set; }
        public int MaxClientCount { get; set; }
        public int MaxClientRequests { get; set; }

        public void Dispose()
        {
            _tcpListener.Stop(); // Stop the session factory TcpListener.
            IsDisposed = true;
        }

        private void ServerTimerTick(object source, ElapsedEventArgs eea)
        {
            Heartbeat.Pulse(SessionList);

            //foreach (Session session in SessionList)
            //{
            //    if (session.RemoteEndpointGuid == Guid.Empty)
            //    {
            //        session.SendString("GUID_GET");
            //    }
            //}
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
                    if (SessionList.Count >= MaxClientCount) throw new Exception("Unable to add client");
                    var clientObject = new Session(0, null, _serverGuid, tcpClient);
                    AddClient(clientObject);
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
            //lock (SessionList)
            //{
            //    foreach (var client in SessionList)
            //    {
            //        client.SendString(broadcastString);
            //    }
            //}
        }

        public void StartClientSessionFactory()
        {
            _clientFactoryThread = new Thread(SessionFactory);
            _clientFactoryThread.Start();
        }

        public void AddClient(Session client)
        {
            lock (SessionList)
            {
                if (client == null) throw new Exception("Invalid client specified for addition");
                SessionList.Add(client);
                ClientCreatedTrigger();
            }
        }

        public void RemoveClient(Session client)
        {
            lock (SessionList)
            {
                if (!SessionList.Contains(client)) throw new Exception("Invalid client specified for removal");
                SessionList.Remove(client);
                ClientRemovedTrigger();
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