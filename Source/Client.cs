using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using netSharp.Events;
using netSharp.Experimental;
using netSharp.Features;

namespace netSharp
{
    public class Client : IDisposable
    {
        private readonly Timer _clientTimer;
        public readonly int MaxSessionCount = 10;
        public readonly List<Session> SessionList = new List<Session>();
        private bool _isDisposed;
        public string ClientGuid;

        public Client()
        {
            ClientGuid = ShortGuid.NewShortGuid();
            _clientTimer = new Timer(1000);
            _clientTimer.Elapsed += ClientTimerTick;
            _clientTimer.Enabled = true;
        }

        public void Dispose()
        {
            _isDisposed = true;
        }

        public event EventHandler<EventDataArgs> SessionRemoved;
        public event EventHandler<EventDataArgs> SessionCreated;
        public event EventHandler<EventDataArgs> SessionPaused;
        public event EventHandler<EventDataArgs> ServerDataReturn;
        public event EventHandler<EventDataArgs> ServerMessage;
        // Event Handler-Trigger Binding
        protected virtual void EventInvocationWrapper(EventDataArgs eventDataArgs,
            EventHandler<EventDataArgs> eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, eventDataArgs);
            }
        }

        public void SessionCreatedTrigger()
        {
            EventInvocationWrapper(new EventDataArgs(), SessionCreated);
        }

        public void SessionPausedTrigger()
        {
            EventInvocationWrapper(new EventDataArgs(), SessionPaused);
        }

        public void SessionRemovedTrigger()
        {
            EventInvocationWrapper(new EventDataArgs(), SessionRemoved);
        }

        public void ServerDataReturnTrigger()
        {
            EventInvocationWrapper(new EventDataArgs(), ServerDataReturn);
        }

        public void ServerMessageTrigger()
        {
            EventInvocationWrapper(new EventDataArgs(), ServerMessage);
        }

        public void ClientTimerTick(object source, ElapsedEventArgs eea)
        {
            Heartbeat.Pulse(SessionList);
        }

        public void NewSession(IPAddress remoteIpAddress, int remotePort)
        {
            var remoteEndpoint = new IPEndPoint(remoteIpAddress, remotePort);
            var session = new Session(1, remoteEndpoint, ClientGuid);
            AddSession(session);
        }

        public void AddSession(Session serverSession)
        {
            if (serverSession == null) throw new ArgumentNullException("serverSession");
            lock (SessionList)
            {
                if (SessionList.Count < MaxSessionCount)
                {
                    SessionList.Add(serverSession);
                    SessionCreatedTrigger();
                    return;
                }
            }
            throw new Exception("Unable to create new session");
        }

        public void RemoveSession(Session serverSession)
        {
            if (serverSession == null) throw new ArgumentNullException("serverSession");
            lock (SessionList)
            {
                if (SessionList.Contains(serverSession))
                {
                    SessionList.Remove(serverSession);
                    SessionRemovedTrigger();
                    return;
                }
            }
            throw new Exception("Unable to remove session");
        }
    }
}