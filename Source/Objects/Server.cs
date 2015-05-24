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
        private int _clientFactoryExceptionCount;
        private Thread _clientFactoryThread;

        public Server(IPEndPoint serverIpEndpoint, int maxClientCount)
        {
            ServerGuid = ShortGuid.NewShortGuid();
            SessionList = new List<Session>();

            _serverTimer = new Timer(1000);
            _serverTimer.Elapsed += ServerTimerTick;
            _serverTimer.Enabled = true;

            MaxClientCount = maxClientCount;
            _tcpListener = new TcpListener(serverIpEndpoint);
            StartClientSessionFactory();
        }

        public string ServerGuid { get; set; }
        public List<Session> SessionList { get; set; }
        public bool IsDisposed { get; set; }
        public int MaxClientCount { get; set; }
        public int MaxClientRequests { get; set; }

        public void Dispose()
        {
            _tcpListener.Stop(); // Stop the session factory TcpListener.
            IsDisposed = true;
        }

        public event EventHandler<NetSharpEventArgs> SessionRemoved;
        public event EventHandler<NetSharpEventArgs> SessionCreated;
        public event EventHandler<NetSharpEventArgs> SessionError;
        public event EventHandler<NetSharpEventArgs> ClientDataReceived;
        // Event Handler-Trigger Binding
        protected virtual void EventInvocationWrapper(NetSharpEventArgs netSharpEventArgs,
            EventHandler<NetSharpEventArgs> eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, netSharpEventArgs);
            }
        }

        public void SessionCreatedTrigger()
        {
            EventInvocationWrapper(new NetSharpEventArgs(), SessionCreated);
        }

        public void SessionRemovedTrigger()
        {
            EventInvocationWrapper(new NetSharpEventArgs(), SessionRemoved);
        }

        public void SessionErrorTrigger()
        {
            EventInvocationWrapper(new NetSharpEventArgs(), SessionRemoved);
        }

        public void ClientDataReceivedTrigger(DataStream dataStream)
        {
            EventInvocationWrapper(new NetSharpEventArgs(dataStream), ClientDataReceived);
        }

        public void HandleSessionDataRecieved(object sender, NetSharpEventArgs e)
        {
            switch (e.DataStream.PayloadType)
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

        private void ServerTimerTick(object source, ElapsedEventArgs eea)
        {
            Keepalive.SessionManager(SessionList);
        }

        public void SendData(object payloadObject, string destinationGuid = null)
        {
            var DataStream = new DataStream(ServerGuid, 11, payloadObject);

            if (destinationGuid != null)
            {
                foreach (var session in SessionList)
                {
                    if (session.RemoteEndpointGuid == destinationGuid)
                    {
                        session.SendData(DataStream);
                    }
                }
            }
            else
            {
                foreach (var session in SessionList)
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
                    var clientObject = new Session(0, null, ServerGuid, tcpClient);
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
                SessionCreatedTrigger();
            }
        }

        public void RemoveClient(Session client)
        {
            lock (SessionList)
            {
                if (!SessionList.Contains(client)) throw new Exception("Invalid client specified for removal");
                SessionList.Remove(client);
                SessionRemovedTrigger();
            }
        }
    }
}