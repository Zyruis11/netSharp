// Copyright (c) 2015 Daniel Elps <daniel.j.elps@gmail.com>
// 
// All rights reserved.
// 
// Redistribution and use of netSharp in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Daniel Elps nor the names of its contributors may be 
//   used to endorse or promote products derived from this software without 
//   specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using netSharp.Connectivity;
using netSharp.Events;

namespace netSharp.Endpoint
{
    public class BaseEndpoint
    {
        public enum EndpointState
        {
            Active,
            Suspended,
            Inactive
        }

        public enum ListeningState
        {
            Listening,
            NotListening
        }

        #region Private Properties

        private Timer endPointTimer { get; set; }
        private Dictionary<string, Session> sessionDictionary { get; set; }

        #endregion

        #region Public Properties

        protected bool IsDisposed { get; set; }
        protected string Guid { get; set; }
        protected int MaxSessions { get; set; }
        protected IPEndPoint ipEndPoint { get; set; }
        protected SessionManager sessionManager { get; set; }
        protected int MaxIdleTime { get; set; }
        protected int MinIdleTime { get; set; }
        protected int SessionManagerIntervalMilliseconds { get; set; }
        protected bool UseMaxIdleTimerScaling { get; set; }

        #endregion

        //TODO: Invoke Session Manager on Timer Tick

        #region DataCommands

        public void ReadDataAsync(Session _session)
        {
            if (_session == null)
                throw new NullReferenceException();
            _session.ReadDataAsync();
        }

        public void SendDataAsync(byte[] _payloadByteArray, Session _session)
        {
            if (_payloadByteArray == null)
                throw new NullReferenceException();
            _session.SendDataAsync();
        }

        #endregion

        #region Session Commands

        public void RemoveSession(Session _session)
        {
            if (_session == null) throw new ArgumentNullException();
            lock (sessionDictionary)
            {
                _session.SessionDataRecieved -= HandleSessionDataRecieved;
                _session.Dispose();
                SessionRemovedTrigger();
            }
        }

        public void AddSession(Session _session)
        {
            if (_session == null)
                throw new ArgumentNullException();

            lock (sessionDictionary)
            {
                _session.SessionDataRecieved += HandleSessionDataRecieved; // Subscribe to the session event handlers
                //session.SessionDataSent
                //TODO: Add the session to the dictionary, using the local guid recieved from the remote endpoint.
                SessionCreatedTrigger();
            }
        }

        public void NewSession(IPEndPoint _remoteIpEndpoint)
        {
            //var session = new Session(_remoteIpEndpoint); //TODO: Implement session factory here.
            //AddSession(session);
        }

        #endregion

        #region Endpoint Events

        public event EventHandler<EndpointEvent> SessionRemoved;
        public event EventHandler<EndpointEvent> SessionCreated;
        public event EventHandler<EndpointEvent> SessionDataRecieved;
        public event EventHandler<EndpointEvent> SessionStateChange;
        public event EventHandler<EndpointEvent> EndpointStateChange;

        protected virtual void TriggerToEventHelper(EndpointEvent _endPointEvent,
            EventHandler<EndpointEvent> _eventHandler)
        {
            _eventHandler?.Invoke(this, _endPointEvent);
        }

        public void SessionCreatedTrigger()
        {
            TriggerToEventHelper(new EndpointEvent("SessionCreated"), SessionCreated);
        }

        public void SessionRemovedTrigger()
        {
            TriggerToEventHelper(new EndpointEvent("SessionRemoved"), SessionRemoved);
        }

        public void SessionDataReceivedTrigger()
        {
            TriggerToEventHelper(new EndpointEvent("SessionDataRecieved"), SessionDataRecieved);
        }

        public void SessionStateChangeTrigger()
        {
            TriggerToEventHelper(new EndpointEvent("SessionStateChange"), SessionStateChange);
        }

        public void EndPointStateChangeTrigger()
        {
            TriggerToEventHelper(new EndpointEvent("EndpointStateChnage"), EndpointStateChange);
        }

        public void HandleSessionDataRecieved(object _sender, EndpointEvent _e)
        {
            //TODO: Route netSharp protocol data and framework user data seperately
        }

        public void HandleSessionDataSent(object _sender, EndpointEvent _e)
        {
            throw new NotImplementedException();
        }

        public void HandleSessionStateChange()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}