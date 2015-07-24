using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using netSharp.Data;
using netSharp.Event_Arguments;
using netSharp.Other;
using netSharp.Sessions;

namespace netSharp.Endpoint
{
    /// <summary>
    /// WARNING: Don't use this class directly, use either Client or Server depending upon your desired functionality.
    /// This is the base class from which the Client and Server classes inherit.
    /// </summary>
    public class BaseEndpoint
    {
        private readonly IPEndPoint _localIpEndPoint;
        private SessionManager _sessionManager { get; } = new SessionManager();
        private Timer EndPointTimer { get; } = new Timer(1000);
        public string LocalGuid { get; } = ShortGuidGenerator.NewShortGuid();
        public Dictionary<string, Session> SessionDictionary { get; } = new Dictionary<string, Session>();
        public bool IsDisposed { get; set; }
        public int MaxSessionCount { get; set; } = 1000;
        //TODO: Invoke Session Manager on Timer Tick


        public void ReadDataAsync(Session session)
        {
            if (session == null)
            {
                throw new NullReferenceException();
            }

            session.ReadDataAsync();
        }

        public void SendDataAsync(byte[] payloadByteArray, Session session)
        {
            if (payloadByteArray == null)
            {
                throw new NullReferenceException();
            }
            var dataStream = new DataStream(LocalGuid, 10, payloadByteArray);
        }

        #region Session Logic

        public void RemoveSession(Session session)
        {
            if (session == null) throw new ArgumentNullException();
            lock (SessionDictionary)
            {
                session.SessionDataRecieved -= HandleSessionDataRecieved;
                session.Dispose();
                SessionRemovedTrigger();
            }
        }

        public void AddSession(Session session)
        {
            if (session == null)
                throw new ArgumentNullException();

            lock (SessionDictionary)
            {
                session.SessionDataRecieved += HandleSessionDataRecieved; // Subscribe to the session event handlers
                session.SessionDataSent
                //TODO: Add the session to the dictionary, using the local guid recieved from the remote endpoint.
                SessionCreatedTrigger();
            }
        }

        public void NewSession(IPEndPoint remoteIpEndpoint)
        {
            var session = new Session(remoteIpEndpoint);
            AddSession(session);
        }

        #endregion
        
        #region Events

        public event EventHandler<EndpointEvents> SessionRemoved;
        public event EventHandler<EndpointEvents> SessionCreated;
        public event EventHandler<EndpointEvents> SessionDataRecieved;
        public event EventHandler<EndpointEvents> SessionLost;

        // Event Handler-Trigger Binding
        protected virtual void EventInvocationWrapper(EndpointEvents netSharpEventArgs,
            EventHandler<EndpointEvents> eventHandler)
        {
            if (eventHandler != null)
            {
                eventHandler(this, netSharpEventArgs);
            }
        }

        public void SessionCreatedTrigger()
        {
            EventInvocationWrapper(new EndpointEvents(), SessionCreated);
        }

        public void SessionRemovedTrigger()
        {
            EventInvocationWrapper(new EndpointEvents(), SessionRemoved);
        }

        public void SessionDataReceivedTrigger(DataStream dataStream)
        {
            EventInvocationWrapper(new EndpointEvents(dataStream), SessionDataRecieved);
        }


        public void HandleSessionDataRecieved(object sender, EndpointEvents e)
        {
            e.SessionReference.IdleTime = 0;

            if (e.SessionReference.RemoteEndpointGuid == "notset")
            {
                e.SessionReference.RemoteEndpointGuid = e.DataStream.Guid;
            }

            //TODO: Handle different types of data more elegantly
        }

        public void HandleSessionDataSent(object sender, EndpointEvents e)
        {
            throw new NotImplementedException();
        }





        #endregion
    }
}