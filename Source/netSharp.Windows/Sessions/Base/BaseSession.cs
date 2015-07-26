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
using System.Net.Sockets;
using System.Threading;
using netSharp.Events;

namespace netSharp.Sessions.Base
{
    public class BaseSession : IDisposable
    {
        public enum SessionState
        {
            Open,
            Opening,
            Closing,
            Closed,
            Error
        }

        public bool IsDisposed { get; set; }
        public string RemoteEndpointGuid { get; set; }
        public string LocalEndpointGuid { get; set; }
        public double IdleTime { get; set; }
        public double MaxIdleTime { get; set; }
        public TcpClient tcpClient { get; set; }
        public CancellationToken cancellationToken { get; set; }
        public CancellationTokenSource cancellationTokenSource { get; set; }
        public string SessionErrorMessage { get; set; }

        public void Dispose()
        {
            cancellationTokenSource.Cancel();
            IsDisposed = true;
        }

        public CancellationToken GetCancellationToken()
        {
            if (cancellationTokenSource != null)
                return cancellationTokenSource.Token;
            return CancellationToken.None;
        }

        #region Session Events

        public event EventHandler<EndpointEvent> SessionDataRecieved;
        public event EventHandler<EndpointEvent> SessionDataSent;
        public event EventHandler<EndpointEvent> SessionStateChange;
        public event EventHandler<EndpointEvent> SessionError;

        private void TriggerToEventHelper(EndpointEvent _endpointEvent,
            EventHandler<EndpointEvent> _eventHandler)
        {
            _eventHandler?.Invoke(this, _endpointEvent);
        }

        public void SessionDataRecievedTrigger()
        {
            TriggerToEventHelper(new EndpointEvent("SessionDataRecieved"), SessionDataRecieved);
        }

        public void SessionDataSentTrigger()
        {
            TriggerToEventHelper(new EndpointEvent("SessionDataSent"), SessionDataSent);
        }

        public void SessionStateChangeTrigger()
        {
            TriggerToEventHelper(new EndpointEvent("SessionStateChange"), SessionStateChange);
        }

        public void SessionErrorTrigger(string _errorMessage)
        {
            TriggerToEventHelper(new EndpointEvent(_errorMessage), SessionError);
        }

        #endregion
    }
}