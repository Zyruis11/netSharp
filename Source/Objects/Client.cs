using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Net;
using System.Timers;
using netSharp.Components;
using netSharp.Events;

namespace netSharp.Objects
{
    public class Client : IDisposable
    {
        private readonly Timer _clientTimer;
        private bool _isDisposed;

        public Client()
        {
            ClientGuid = ShortGuidGenerator.NewShortGuid();
            SessionList = new List<Session>();

            _clientTimer = new Timer(10000);
            _clientTimer.Elapsed += ClientTimerTick;
            _clientTimer.Enabled = true;
        }

        public List<Session> SessionList { get; set; }
        public string ClientGuid { get; set; }

        public void Dispose()
        {
            _isDisposed = true;
        }

        public event EventHandler<NetSharpEventArgs> SessionRemoved;
        public event EventHandler<NetSharpEventArgs> SessionCreated;
        public event EventHandler<NetSharpEventArgs> SessionError;
        public event EventHandler<NetSharpEventArgs> ServerDataRecieved;
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

        public void SessionErrorTrigger(string errorMessage)
        {
            EventInvocationWrapper(new NetSharpEventArgs(null, null, errorMessage), SessionError);
        }

        public void ServerDataReceivedTrigger(DataStream dataStream)
        {
            EventInvocationWrapper(new NetSharpEventArgs(dataStream), ServerDataRecieved);
        }

        public void HandleSessionDataRecieved(object sender, NetSharpEventArgs e)
        {
            switch (e.DataStream.PayloadType)
            {
                case 0: // Hello
                {
                    SessionManager.ProcessRecievedHello(e.SessionReference);
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

        public void HandleSessionErrorRecieved(object sender, NetSharpEventArgs e)
        {
        }

        public void ClientTimerTick(object source, ElapsedEventArgs eea)
        {
            SessionManager.SessionStateEngine(SessionList);
        }

        public void SendData(object payloadObject, string destinationGuid = null)
        {
            var DataStream = new DataStream(ClientGuid, 11, payloadObject);

            if (destinationGuid.Length == 4) // If this is a sticky request i.e. a request intended for one server.
            {
                foreach (var session in SessionList)
                {
                    if (session.RemoteEndpointGuid == destinationGuid)
                    {
                        session.SendData(DataStream);
                    }
                }
                return;
            }

            if (destinationGuid == "ALLSERVERS")
            {
                foreach (var session in SessionList)
                {
                    session.SendData(DataStream);
                }
            }

            var BestCost = byte.MaxValue;
            Session BestCostSession = null;

            foreach (var session in SessionList)
            {
                if (session.Cost < BestCost)
                {
                    BestCost = session.Cost;
                    BestCostSession = session;
                }
            }
            if (BestCostSession == null)
            {
                return;
            }

            BestCostSession.SendData(DataStream);
        }

        public void NewSession(IPEndPoint remoteIpEndpoint)
        {
            var session = new Session(1, remoteIpEndpoint, ClientGuid);
            AddSession(session);
        }

        public void AddSession(Session serverSession)
        {
            if (serverSession == null) throw new ArgumentNullException();
            lock (SessionList)
            {
                SessionList.Add(serverSession);
                serverSession.SessionDataRecieved += HandleSessionDataRecieved;
                SessionCreatedTrigger();
            }
        }

        public void RemoveSession(Session serverSession)
        {
            if (serverSession == null) throw new ArgumentNullException("serverSession");
            lock (SessionList)
            {
                if (!SessionList.Contains(serverSession)) throw new InstanceNotFoundException();
                SessionList.Remove(serverSession);
                SessionRemovedTrigger();
            }
        }
    }
}