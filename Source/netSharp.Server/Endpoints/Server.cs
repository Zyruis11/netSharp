using System;
using System.Collections.Generic;
using System.Data;
using System.Management.Instrumentation;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using netSharp.Core.Data;
using netSharp.Core.Helpers;
using netSharp.Server.Connectivity;
using netSharp.Server.Events;

namespace netSharp.Server.Endpoints
{
    public class Server : IDisposable
    {
        private readonly Timer _serverTimer;
        private readonly TcpListener _tcpListener;

        public Server(IPEndPoint serverIpEndpoint, int maxClientCount)
        {
            ServerGuid = ShortGuidGenerator.NewShortGuid();
            SessionList = new List<ServerSession>();

            _serverTimer = new Timer(1000);
            _serverTimer.Elapsed += ServerTimerTick;
            _serverTimer.Enabled = true;

            MaxClientCount = maxClientCount;
            _tcpListener = new TcpListener(serverIpEndpoint);
            StartClientSessionFactory();
        }

        public string ServerGuid { get; set; }
        public List<ServerSession> SessionList { get; set; }
        public bool IsDisposed { get; set; }
        public int MaxClientCount { get; set; }

        public void Dispose()
        {
            _tcpListener.Stop(); // Stop the session factory TcpListener.
            IsDisposed = true;
        }

        public event EventHandler<ServerEvents> SessionRemoved;
        public event EventHandler<ServerEvents> SessionCreated;
        public event EventHandler<ServerEvents> ClientDataReceived;
        // Event Handler-Trigger Binding
        protected virtual void EventInvocationWrapper(ServerEvents netSharpEventArgs,
            EventHandler<ServerEvents> eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, netSharpEventArgs);
            }
        }

        public void SessionCreatedTrigger()
        {
            EventInvocationWrapper(new ServerEvents(), SessionCreated);
        }

        public void SessionRemovedTrigger()
        {
            EventInvocationWrapper(new ServerEvents(), SessionRemoved);
        }

        public void SessionErrorTrigger()
        {
            EventInvocationWrapper(new ServerEvents(), SessionRemoved);
        }

        public void ClientDataReceivedTrigger(DataStream dataStream)
        {
            EventInvocationWrapper(new ServerEvents(dataStream), ClientDataReceived);
        }

        public void HandleSessionDataRecieved(object sender, ServerEvents e)
        {
            e.SessionReference.IdleTime = 0;

            if (e.SessionReference.RemoteEndpointGuid == "notset")
            {
                e.SessionReference.RemoteEndpointGuid = e.DataStream.Guid;
            }

            switch (e.DataStream.PayloadType)
            {
                case 0: // Hello
                {
                    break;
                }
                case 11: // Application Data
                {
                    e.SessionReference.IdleTime = 0;
                    ClientDataReceivedTrigger(e.DataStream);
                    break;
                }
            }
        }

        private void ServerTimerTick(object source, ElapsedEventArgs eea)
        {
            ServerSessionManager.SessionStateEngine(SessionList, MaxClientCount);
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
                    session.StreamWriterAsync(DataStream);
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
                var clientObject = new ServerSession(0, null, ServerGuid, tcpClient);
                AddSession(clientObject);
            }
            _tcpListener.Stop();
        }

        public void StartClientSessionFactory()
        {
            SessionFactory();
        }

        public void AddSession(ServerSession session)
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

        public void RemoveSession(ServerSession session)
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