using System;
using System.Collections.Generic;
using System.Data;
using System.Management.Instrumentation;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;
using netSharp.Components;
using netSharp.Events;

namespace netSharp.Objects
{
    public class Server : IDisposable
    {
        private readonly IPAddress _listenAddress;
        private readonly int _listenPort;
        private readonly Timer _serverTimer;
        private readonly TcpListener _tcpListener;
        private int _clientFactoryExceptionCount;

        public Server(IPEndPoint serverIpEndpoint, int maxClientCount)
        {
            ServerGuid = ShortGuidGenerator.NewShortGuid();
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
            e.SessionReference.IdleTime = 0;

            switch (e.DataStream.PayloadType)
            {
                case 0: // Hello
                {
                    SessionManager.ProcessRecievedHello(e.SessionReference);
                    break;
                }
                case 11: // Application Data
                {
                    ClientDataReceivedTrigger(e.DataStream);
                    break;
                }
            }
        }

        public void HandleSessionErrorRecieved(object sender, NetSharpEventArgs e)
        {
        }

        private void ServerTimerTick(object source, ElapsedEventArgs eea)
        {
            SessionManager.SessionStateEngine(SessionList, MaxClientCount);
        }

        public void SendData(byte[] payloadObject, string destinationGuid = "")
        {
            var DataStream = new DataStream(ServerGuid, 11, payloadObject);

            if (destinationGuid == null)
            {
                throw new ArgumentNullException();
            }

            foreach (var session in SessionList)
            {
                if (session.RemoteEndpointGuid == destinationGuid)
                {
                    session.SendData(DataStream);
                }
            }
        }

        private async void SessionFactory()
        {
            _tcpListener.Start();

            while (!IsDisposed)
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                if (SessionList.Count >= MaxClientCount) throw new Exception("Unable to add session");
                var clientObject = new Session(0, null, ServerGuid, tcpClient);
                AddSession(clientObject);
            }
            _tcpListener.Stop();
        }

        public void StartClientSessionFactory()
        {
            SessionFactory();
        }

        public void AddSession(Session session)
        {
            if (session == null) throw new ArgumentNullException();
            lock (SessionList)
            {
                if (SessionList.Contains(session)) throw new DuplicateNameException();
                SessionList.Add(session);
                session.SessionDataRecieved += HandleSessionDataRecieved;
                SessionCreatedTrigger();
            }
        }

        public void RemoveSession(Session session)
        {
            if (session == null) throw new ArgumentNullException();
            lock (SessionList)
            {
                if (!SessionList.Contains(session)) throw new InstanceNotFoundException();
                session.SessionDataRecieved -= HandleSessionDataRecieved;
                SessionList.Remove(session);
                SessionRemovedTrigger();
            }
        }
    }
}