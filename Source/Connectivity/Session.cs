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
using System.Net;
using System.Net.Sockets;
using System.Threading;
using netSharp.Configuration;
using netSharp.Connectivity.Base;
using netSharp.Factories.DataStream;
using netSharp.Other;

namespace netSharp.Connectivity
{
    public sealed class Session : BaseSession
    {
        public Session(TcpClient _tcpClient, IPEndPoint _ipEndpoint, CancellationTokenSource _cancellationTokenSource,
            SessionConfiguration _sessionConfiguration = null)
        {
            tcpClient = _tcpClient;

            if (!tcpClient.Connected)
                tcpClient.Connect(_ipEndpoint);

            ApplySessionConfiguration(_sessionConfiguration ?? new SessionConfiguration());

            GetCancellationToken(_cancellationTokenSource);

            LocalEndpointGuid = ShortGuid.New();

            SessionState = (byte) SessionStates.Opening;
                // Set the session state to opening, it will transition to Open once the RemoteEndpointGuid has been received.

            ReadDataAsync(); // Begin a non-blocking read on the Tcpclient of the session.
        }

        private void ApplySessionConfiguration(SessionConfiguration _sessionConfiguration)
        {
            if (_sessionConfiguration.MaxReconnectAttempts > 0)
                MaxReconnectAttempts = _sessionConfiguration.MaxReconnectAttempts;

            if (_sessionConfiguration.ReconnectionCounterResetInterval >= 10)
                ReconnectionCounterResetInterval = _sessionConfiguration.ReconnectionCounterResetInterval;

            if (_sessionConfiguration.ConnectionTimeout >= 1000)
                ConnectionTimeout = _sessionConfiguration.ConnectionTimeout;

            if (_sessionConfiguration.KeepaliveSendInterval >= 1)
                KeepaliveSendInterval = _sessionConfiguration.KeepaliveSendInterval;

            if (_sessionConfiguration.KeepaliveDeadTime >= 4)
                KeepaliveDeadTime = _sessionConfiguration.KeepaliveDeadTime;

            if (_sessionConfiguration.MaxDataKeyValueStoreDepth <= 25)
                MaxDataKeyValueStoreDepth = _sessionConfiguration.MaxDataKeyValueStoreDepth;

            UseKeepalives = _sessionConfiguration.UseKeepalives;
        }

        public async void ReadDataAsync()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var networkStream = tcpClient.GetStream();
                var protocolInfoBuffer = new byte[10];
                var initialBytesRead = 0;

                initialBytesRead =
                    await networkStream.ReadAsync(protocolInfoBuffer, 0, protocolInfoBuffer.Length, cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    var dataStream = DataStreamFactory.InitializeStreamObject(protocolInfoBuffer);

                    var payloadBytesRead = 0;
                    var payloadBytesRemaining = dataStream.PayloadLength;
                    dataStream.PayloadByteArray = new byte[dataStream.PayloadLength];

                    while (payloadBytesRead < dataStream.PayloadLength)
                    {
                        try
                        {
                            payloadBytesRead +=
                                await
                                    networkStream.ReadAsync(dataStream.PayloadByteArray, 0, payloadBytesRemaining,
                                        cancellationToken);
                        }
                        catch
                        {
                            break;
                        }

                        if (payloadBytesRead == 0)
                        {
                            //IdleTime = MaxIdleTime;
                            return;
                        }
                        payloadBytesRemaining -= payloadBytesRead;
                    }
                    SessionDataRecievedTrigger();
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                tcpClient.Close();
            }
        }

        public async void SendDataAsync()
        {
            if (tcpClient.Connected)
            {
                try
                {
                    var networkStream = tcpClient.GetStream();
                    await networkStream.WriteAsync(new byte[100], 0, 100); //TODO: Fix this.
                    SessionDataSentTrigger();
                }
                catch (Exception _exception)
                {
                    SessionErrorTrigger(_exception.Message);
                }
            }
            else
            {
                SessionErrorTrigger("Session is not connected.");
            }
        }
    }
}