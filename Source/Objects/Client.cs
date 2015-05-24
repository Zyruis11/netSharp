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
        private bool _isDisposed;

        public Client()
        {
            ClientGuid = ShortGuid.NewShortGuid();
            SessionList = new List<Session>();

            _clientTimer = new Timer(1000);
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
                    Keepalive.ProcessRecievedHello(e.SessionReference);
                    break;
                }
                case 11: // Application Data
                {
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
            Keepalive.SessionManager(SessionList);
        }

        public void SendData(object payloadObject, string destinationGuid = null)
        {
            var DataStream = new DataStream(ClientGuid, 11, payloadObject);

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