using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using netSharp.Components;
using netSharp.Events;

namespace netSharp.Objects
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

        public event EventHandler<SessionEventArgs> SessionRemoved;
        public event EventHandler<SessionEventArgs> SessionCreated;
        public event EventHandler<SessionEventArgs> SessionPaused;
        public event EventHandler<SessionEventArgs> ServerDataReturn;
        public event EventHandler<SessionEventArgs> ServerMessage;
        // Event Handler-Trigger Binding
        protected virtual void EventInvocationWrapper(SessionEventArgs sessionEventArgs,
            EventHandler<SessionEventArgs> eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, sessionEventArgs);
            }
        }

        public void SessionCreatedTrigger()
        {
            EventInvocationWrapper(new SessionEventArgs(), SessionCreated);
        }

        public void SessionPausedTrigger()
        {
            EventInvocationWrapper(new SessionEventArgs(), SessionPaused);
        }

        public void SessionRemovedTrigger()
        {
            EventInvocationWrapper(new SessionEventArgs(), SessionRemoved);
        }

        public void RecievedDataTrigger(DataStream dataStream)
        {
            EventInvocationWrapper(new SessionEventArgs(dataStream), ServerMessage);
        }

        public void HandleSessionDataRecieved(object sender, SessionEventArgs e)
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
                        RecievedDataTrigger(e.DataStream);
                        break;
                    }
            }
        }

        public void ClientTimerTick(object source, ElapsedEventArgs eea)
        {
            Keepalive.SessionManager(SessionList);
        }

        public void SendData(object payloadObject, string destinationGuid = null)
        {
            DataStream DataStream = new DataStream(ClientGuid, 11, payloadObject);

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
                    serverSession.SessionDataRecieved += HandleSessionDataRecieved;
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