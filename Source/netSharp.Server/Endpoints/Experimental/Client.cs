using System;
using System.Collections.Generic;
using System.Data;
using System.Management.Instrumentation;
using System.Net;
using System.Timers;
using netSharp.Core.Data;
using netSharp.Core.Helpers;
using netSharp.Server.Connectivity;
using netSharp.Server.Events;

namespace netSharp.Server.Endpoints.Experimental
{
    public class Client : IDisposable
    {
        private readonly Timer _clientTimer;

        public Client()
        {
            ClientGuid = ShortGuidGenerator.NewShortGuid();
            SessionList = new List<Session>();

            _clientTimer = new Timer(5000);
            _clientTimer.Elapsed += ClientTimerTick;
            _clientTimer.Enabled = true;
        }

        public bool IsDisposed { get; set; }
        public List<Session> SessionList { get; set; }
        public string ClientGuid { get; set; }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public event EventHandler<ServerEvents> SessionRemoved;
        public event EventHandler<ServerEvents> SessionCreated;
        public event EventHandler<ServerEvents> ServerDataRecieved;
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

        public void ServerDataReceivedTrigger(DataStream dataStream)
        {
            EventInvocationWrapper(new ServerEvents(dataStream), ServerDataRecieved);
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
                    ServerDataReceivedTrigger(e.DataStream);
                    break;
                }
            }
        }

        public void ClientTimerTick(object source, ElapsedEventArgs eea)
        {
            SessionManager.SessionStateEngine(SessionList);
        }

        public void SendData(byte[] payloadObject, string destinationGuid = null)
        {
            var DataStream = new DataStream(ClientGuid, 11, payloadObject);

            if (destinationGuid.Length == 4) // If this is a sticky request i.e. a request intended for one server.
            {
                foreach (var session in SessionList)
                {
                    if (session.RemoteEndpointGuid == destinationGuid)
                    {
                        session.SendDataAsync(DataStream);
                    }
                }
                return;
            }

            if (destinationGuid == "ALLSERVERS")
            {
                foreach (var session in SessionList)
                {
                    session.SendDataAsync(DataStream);
                }
            }
        }

        public void NewSession(IPEndPoint remoteIpEndpoint)
        {
            var session = new Session(remoteIpEndpoint);
            AddSession(session);
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