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

using netSharp.Configuration.Base;

namespace netSharp.Configuration
{
    public sealed class ServerConfiguration : BaseConfiguration
    {
        /// <summary>
        ///     Gets or sets the maximum number of peers allowed per server cluster.
        /// </summary>
        public int MaxClusterSize { get; set; } = 2;

        /// <summary>
        ///     Gets or sets the maximum number of clusters that the Endpoint may belong to.
        /// </summary>
        public int MaxUniqueClusters { get; set; } = 1;

        /// <summary>
        ///     Gets or sets the maximum number of connections allowed to unique servers.
        /// </summary>
        public int MaxServerUniqueConnections { get; set; } = 10;

        /// <summary>
        ///     Gets or sets the maximum number of connections allowed to a single server
        /// </summary>
        public int MaxSessionsPerRemoteEndPoint { get; set; } = 2;

        /// <summary>
        ///     Override of the base MaxSessions property, by default this allows the server to accept 10x the connections of a
        ///     client.
        /// </summary>
        public override int MaxSessions { get; set; } = 100;
    }
}