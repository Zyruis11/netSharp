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
using netSharp.Server.Interfaces;

namespace netSharp.Server.Endpoints
{
    public class Server : IDisposable, IServer
    {
        private readonly Timer _serverTimer;
        private readonly TcpListener _tcpListener;

        public Server(IPEndPoint serverIpEndpoint, int maxClientCount)
        {
            ServerGuid = ShortGuidGenerator.NewShortGuid();
            SessionList = new List<Session>();

            _serverTimer = new Timer(1000);
            _serverTimer.Elapsed += ServerTimerTick;
            _serverTimer.Enabled = true;

            MaxClientCount = maxClientCount;
            _tcpListener = new TcpListener(serverIpEndpoint);
            StartListener();
        }

        public string ServerGuid { get; set; }
        public List<Session> SessionList { get; set; }
        public bool IsDisposed { get; set; }
        public int MaxClientCount { get; set; }

        public void Dispose()
        {
            _tcpListener.Stop(); // Stop the session factory ClientListener.
            IsDisposed = true;
        }

        public void SendDataAsync(byte[] payloadObject, Session session)
        {
            var DataStream = new DataStream(ServerGuid, 10, payloadObject);


        }

        public void StartListener()
        {
            ClientListener(true);
        }

        public void StopListener()
        {
            ClientListener(false);
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
                case 0: // netSharp Data
                {
                    //TODO: Process netSharp data
                    break;
                }
                case 10: // Application Data
                {
                    e.SessionReference.IdleTime = 0;
                    ClientDataReceivedTrigger(e.DataStream);
                    break;
                }
            }
        }

        private void ServerTimerTick(object source, ElapsedEventArgs eea)
        {
            SessionManager.SessionStateEngine(SessionList, MaxClientCount);
        }

        private async void ClientListener(bool isStarting)
        {
            if (isStarting)
            {
                _tcpListener.Start();

                while (!IsDisposed)
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();
                    if (SessionList.Count >= MaxClientCount) throw new Exception("Unable to add session");
                    var clientObject = new Session(tcpClient);
                    AddSession(clientObject);
                }
            }
            else
            {
                _tcpListener.Stop();
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
    }
}