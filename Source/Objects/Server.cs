using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using netSharp.Components;
using netSharp.Events;
using Timer = System.Timers.Timer;

namespace netSharp.Objects
{
    public class Server : IDisposable
    {
        private readonly IPAddress _listenAddress;
        private readonly int _listenPort;
        private readonly Timer _serverTimer;
        private readonly TcpListener _tcpListener;
        private readonly string _serverGuid;
        private int _clientFactoryExceptionCount;
        private Thread _clientFactoryThread;

        public event EventHandler<SessionEventArgs> ClientRemoved;
        public event EventHandler<SessionEventArgs> ClientCreated;
        public event EventHandler<SessionEventArgs> ClientDataReceived;
        public event EventHandler<SessionEventArgs> ListenerPaused;
        public event EventHandler<SessionEventArgs> NewClientRequest;

        // Event Handler-Trigger Binding
        protected virtual void EventInvocationWrapper(SessionEventArgs sessionEventArgs,
            EventHandler<SessionEventArgs> eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, sessionEventArgs);
            }
        }

        public void ClientCreatedTrigger()
        {
            EventInvocationWrapper(new SessionEventArgs(), ClientCreated);
        }

        public void ListenerPausedTrigger()
        {
            EventInvocationWrapper(new SessionEventArgs(), ListenerPaused);
        }

        public void ClientRemovedTrigger()
        {
            EventInvocationWrapper(new SessionEventArgs(), ClientRemoved);
        }

        public void ClientDataReceivedTrigger(DataStream dataStream)
        {
            EventInvocationWrapper(new SessionEventArgs(dataStream), ClientDataReceived);
        }

        public void HandleSessionDataRecieved(object sender, SessionEventArgs e)
        {
           switch(e.DataStream.PayloadType)
            {
               case 0: // Hello
                {
                    Keepalive.ProcessRecievedHello(e.SessionReference);
                    break;
                }
               case 11: // Application Data
                {
                    ClientDataReceivedTrigger(e.DataStream);
                    break;
                }
            }
        }

        public Server(string serverBindAddr, int serverBindPort, int maxClientRequests, int maxClientCount)
        {
            _listenAddress = IPAddress.Parse(serverBindAddr);
            _listenPort = serverBindPort;

            _tcpListener = new TcpListener(_listenAddress, _listenPort);
            SessionList = new List<Session>();
            _serverTimer = new Timer(1000);
            _serverTimer.Elapsed += ServerTimerTick;
            _serverTimer.Enabled = true;

            MaxClientRequests = maxClientRequests;
            MaxClientCount = maxClientCount;

            _listenAddress = IPAddress.Parse(serverBindAddr);
            _listenPort = serverBindPort;

            _serverGuid = ShortGuid.NewShortGuid();

            StartClientSessionFactory();
        }

        public List<Session> SessionList { get; set; }
        public bool IsDisposed { get; set; }
        public int MaxClientCount { get; set; }
        public int MaxClientRequests { get; set; }

        public void Dispose()
        {
            _tcpListener.Stop(); // Stop the session factory TcpListener.
            IsDisposed = true;
        }

        private void ServerTimerTick(object source, ElapsedEventArgs eea)
        {
            Keepalive.SessionManager(SessionList);
        }


        public void SendData(object payloadObject, string destinationGuid = null)
        {
            DataStream DataStream = new DataStream(_serverGuid, 11, payloadObject);

            if (destinationGuid != null)
            {
                foreach (Session session in SessionList)
                {
                    if (session.RemoteEndpointGuid == destinationGuid)
                    {
                        session.SendData(DataStream);
                    }
                }
            }
            else
            {
                foreach (Session session in SessionList)
                {
                    session.SendData(DataStream);
                }
            }
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
                    clientObject.SessionDataRecieved += HandleSessionDataRecieved;
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
    }
}