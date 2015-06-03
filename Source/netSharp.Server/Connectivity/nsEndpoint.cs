using System;
using System.Collections.Generic;
using System.Data;
using System.Management.Instrumentation;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using netSharp.Core.Data;
using netSharp.Core.Helpers;
using netSharp.Server.Events;
using netSharp.Server.Interfaces;

namespace netSharp.Server.Connectivity
{
    public class NsEndpoint : IDisposable, IEndpoint
    {
        private readonly Timer _endpointTimer;
        private readonly IPEndPoint _localIpEndPoint;
        private readonly SessionManager _sessionManager;
        private readonly TcpListener _tcpListener;

        public NsEndpoint(bool isServer, IPEndPoint localIpEndPoint = null, int maxSessionCount = 10)
        {
            LocalGuid = ShortGuid.NewShortGuid();
            SessionList = new List<Session>();
            MaxSessionCount = maxSessionCount;
            IsServer = isServer;
            _sessionManager = new SessionManager(this);

            if (IsServer)
            {
                _localIpEndPoint = localIpEndPoint;
                _tcpListener = new TcpListener(_localIpEndPoint);
                StartListener();
            }

            _endpointTimer = new Timer(1000);
            _endpointTimer.Elapsed += EndpointTimerTick;
            _endpointTimer.Enabled = true;
        }

        public bool IsServer { get; set; }
        public string LocalGuid { get; set; }
        public List<Session> SessionList { get; set; }
        public bool IsDisposed { get; set; }
        public int MaxSessionCount { get; set; }

        public void Dispose()
        {
            _endpointTimer.Stop();
            RemoveAllSessions();
            IsDisposed = true;
            InternalTcpConnection();
        }

        public void ReadDataAsync(Session session)
        {
            if (session == null)
            {
                throw new NullReferenceException();
            }

            session.ReadDataAsync();
        }

        public void StartListener()
        {
            NewSessionListener();
        }

        public void RemoveSession(Session session)
        {
            if (session == null) throw new ArgumentNullException();
            lock (SessionList)
            {
                if (!SessionList.Contains(session)) throw new InstanceNotFoundException();
                session.SessionDataRecieved -= HandleSessionDataRecieved;
                SessionList.Remove(session);
                session.Dispose();
                SessionRemovedTrigger();
            }
        }

        public void AddSession(Session session)
        {
            if (session == null) throw new ArgumentNullException();
            lock (SessionList)
            {
                if (SessionList.Contains(session)) throw new DuplicateNameException();
                session.SessionDataRecieved += HandleSessionDataRecieved;
                SessionList.Add(session);
                SessionCreatedTrigger();
            }
        }

        public void SendDataAsync(byte[] payloadByteArray, Session session = null)
        {
            if (payloadByteArray == null)
            {
                throw new NullReferenceException();
            }

            var dataStream = new DataStream(LocalGuid, 10, payloadByteArray);

            if (session == null)
            {
                foreach (var sessionInstance in SessionList)
                {
                    sessionInstance.SendDataAsync(dataStream);
                }
            }
            else
            {
                session.SendDataAsync(dataStream);
            }
        }

        public void RemoveAllSessions()
        {
            lock (SessionList)
            {
                var sessionsToDispose = new List<Session>();

                foreach (var session in SessionList)
                {
                    sessionsToDispose.Add(session);
                }

                foreach (var session in sessionsToDispose)
                {
                    session.Dispose();
                }
            }
        }

        public event EventHandler<ServerEvents> SessionRemoved;
        public event EventHandler<ServerEvents> SessionCreated;
        public event EventHandler<ServerEvents> SessionDataRecieved;
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

        public void SessionDataReceivedTrigger(DataStream dataStream)
        {
            EventInvocationWrapper(new ServerEvents(dataStream), SessionDataRecieved);
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
                case 0: // netSharp Data
                {
                    //TODO: Process netSharp data
                    break;
                }
                case 10: // Application Data
                {
                    e.SessionReference.IdleTime = 0;
                    SessionDataReceivedTrigger(e.DataStream);
                    break;
                }
            }
        }

        private void EndpointTimerTick(object source, ElapsedEventArgs eea)
        {
            _sessionManager.TimerTick();
        }

        public void NewSession(IPEndPoint remoteIpEndpoint)
        {
            var session = new Session(remoteIpEndpoint);
            AddSession(session);
        }

        private async void NewSessionListener()
        {
            _tcpListener.Start();

            while (!IsDisposed)
            {
                var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                if (!IsDisposed)
                {
                    if (SessionList.Count >= MaxSessionCount) throw new Exception("Unable to add session");
                    var clientObject = new Session(tcpClient);
                    AddSession(clientObject);
                }
            }

            _tcpListener.Stop();
        }

        private void InternalTcpConnection()
        {
            var _tcpClient = new TcpClient();
            _tcpClient.Connect(_localIpEndPoint);
            _tcpClient.Close();
        }
    }
}