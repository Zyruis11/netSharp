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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using netSharp.Configuration;
using netSharp.Other;
using netSharp.Sessions;

namespace netSharp.Endpoint
{
    public sealed class Server : BaseEndpoint, IDisposable
    {
        private TcpListener tcpListener { get; set; }
        public bool IsListening { get; set; }
        public int MaxClusterSize { get; set; }
        public int MaxSessionsPerRemoteEndPoint { get; set; }
        public int MaxServerUniqueSessions { get; set; }
        public int MaxUniqueClusters { get; set; }

        public Server(SessionManager _sessionManager, IPEndPoint _ipEndPoint, TcpListener _tcpListener, ServerConfiguration _serverConfiguration = null)
        {
            sessionManager = _sessionManager;
            ipEndPoint = _ipEndPoint;
            tcpListener = _tcpListener;

            ApplyServerConfiguration(_serverConfiguration ?? new ServerConfiguration());

            Guid = ShortGuidGenerator.New();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void StartListener()
        {
            IsListening = true;
            tcpListener.Start();
        }

        public void StopListener()
        {
            IsListening = false;
            tcpListener.Stop();          
        }

        private async void SessionListener()
        {     
            while (IsListening)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
                if (!IsDisposed)
                {
                    //if (SessionDictionary.Count >= MaxSessions) //TODO: Close longest inactive session to acomodate new session
                    //var session = new Session(tcpClient); //TODO: Implement session factory pattern
                    //AddSession(session);
                }
            }
        }

        private void ApplyServerConfiguration(ServerConfiguration _serverConfiguration)
        {
            if (_serverConfiguration == null) return;

            if (_serverConfiguration.MaxClusterSize > 0)
                MaxClusterSize = _serverConfiguration.MaxClusterSize;

            if (_serverConfiguration.MaxUniqueClusters > 0)
                MaxUniqueClusters = _serverConfiguration.MaxUniqueClusters;

            if (_serverConfiguration.MaxServerUniqueConnections > 0)
                MaxServerUniqueSessions = _serverConfiguration.MaxServerUniqueConnections;

            if (_serverConfiguration.MaxSessionsPerRemoteEndPoint > 0)
                MaxSessionsPerRemoteEndPoint = _serverConfiguration.MaxSessionsPerRemoteEndPoint;

            if (_serverConfiguration.MaxSessions > 0)
                MaxSessions = _serverConfiguration.MaxSessions;

            if (_serverConfiguration.MinIdleTime > 0)
                MinIdleTime = _serverConfiguration.MinIdleTime;

            if (_serverConfiguration.MaxIdleTime > MinIdleTime)
                MaxIdleTime = _serverConfiguration.MaxIdleTime;

            if (_serverConfiguration.SessionManagerIntervalMilliseconds > 500)
                SessionManagerIntervalMilliseconds = _serverConfiguration.SessionManagerIntervalMilliseconds;

            UseKeepalives = _serverConfiguration.UseKeepalives;
        }
    }
}
