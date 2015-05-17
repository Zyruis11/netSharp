using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using Library.Networking.TCP.Events;
using Library.Networking.TCP.Features;

namespace Library.Networking.TCP
{
    public class Client : IDisposable
    {
        public readonly int _maxSessionCount = 10;
        public readonly List<Session> _sessionList = new List<Session>();
        private Timer _clientTimer;
        private bool _isDisposed;
        public Guid ClientGuid;

        public void Dispose()
        {
            _isDisposed = true;
        }

        // Event Handlers
        public event EventHandler<TcpEventArgs> SessionRemoved;
        public event EventHandler<TcpEventArgs> ClientIntialized;
        public event EventHandler<TcpEventArgs> ServerDataRecieved;
        public event EventHandler<TcpEventArgs> SessionCreated;
        // Event Handler-Trigger Binding
        protected virtual void EventTriggerToEventHandlerBindingController(TcpEventArgs tcpEventArgs,
            EventHandler<TcpEventArgs> eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, tcpEventArgs);
            }
        }

        // Event Triggers
        public void SessionCreatedTrigger()
        {
            EventTriggerToEventHandlerBindingController(new TcpEventArgs("New Session Created"), SessionCreated);
        }

        public void Intialize()
        {
            ClientGuid = Guid.NewGuid();
            _clientTimer = new Timer(1000);
            _clientTimer.Elapsed += ClientTimerTick;
            _clientTimer.Enabled = true;
        }

        public void ClientTimerTick(object source, ElapsedEventArgs eea)
        {
            Heartbeat.Pulse(_sessionList);

            foreach (Session session in _sessionList)
            {
                if (session.RemoteEndpointGuid == Guid.Empty)
                {
                    session.SendString("GUID_GET");
                }
            }
        }

        public void NewSession(IPAddress remoteIpAddress, int remotePort)
        {
            var remoteEndpoint = new IPEndPoint(remoteIpAddress, remotePort);
            var session = new Session(1, remoteEndpoint, ClientGuid);
            if (AddSession(session))
            {
                SessionCreatedTrigger();
            }
        }

        public bool AddSession(Session serverSession)
        {
            if (serverSession == null) throw new ArgumentNullException("serverSession");
            lock (_sessionList)
            {
                if (_sessionList.Count < _maxSessionCount)
                {
                    _sessionList.Add(serverSession);

                    return true;
                }
            }
            return false;
        }

        public void RemoveSession(Session serverSession)
        {
            lock (_sessionList)
            {
                if (_sessionList.Contains(serverSession))
                {
                    _sessionList.Remove(serverSession);
                }
            }
        }
    }
}