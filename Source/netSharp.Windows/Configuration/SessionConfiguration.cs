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

namespace netSharp.Configuration
{
    public class SessionConfiguration
    {
        #region Properites

        /// <summary>
        ///     Gets or sets the maximum number of times that a session will attempt to reconnect to its remote endpoint before
        ///     it is marked Closed.
        /// </summary>
        public int MaxReconnectAttempts { get; set; } = 5;

        /// <summary>
        ///     Gets or sets the length of time that the reconnect counter will wait before resetting to 0. If reconnect attempts go
        ///     over the MaxReconnectAttempts interval before this value reaches 0, the session will be marked Closed and disposed by
        ///     the session manager.
        /// </summary>
        public int ReconnectionCounterResetInterval { get; set; } = 30;

        /// <summary>
        ///     Gets or sets the length of time that the Session will attempt to connect or reconnect to the remote endpoint before timing
        ///     out and marking itself as Closed.
        /// </summary>
        public int ConnectionTimeout { get; set; } = 2500;

        /// <summary>
        ///     Gets or sets the bool allowing the Endpoint's Session Manager to use keepalives during session maintenance.
        ///     Note: Keepalives do not reset the Idle timer when received, a session using Keepalives will still timeout.
        /// </summary>
        public bool UseKeepalives { get; set; } = true;

        /// <summary>
        ///     Gets or sets the interval for sending keepalives to session peers. Only used if Keepalives are enabled.
        /// </summary>
        public int KeepaliveSendInterval { get; set; } = 15;

        /// <summary>
        ///     Gets or sets the time that the session will go without recieving keepalives from the remote peer before marking itself as closed.
        ///     Only used if keepalives are enabled.
        /// </summary>
        public int KeepaliveDeadTime { get; set; } = 60;

        /// <summary>
        ///     Gets or sets the maximum number of members allowed in the session KeyValue store, this KVS contains recieved messages and message ID's.
        /// </summary>
        public int MaxDataKeyValueStoreDepth { get; set; } = 10;

        #endregion
    }
}