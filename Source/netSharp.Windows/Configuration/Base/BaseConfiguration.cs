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
namespace netSharp.Configuration.Base
{
    public class BaseConfiguration
    {
        /// <summary>
        /// Gets or sets the maximum length of time in seconds that an idle session will be maintained.
        /// </summary>
        public int MaxIdleTime { get; set; } = 600;

        /// <summary>
        /// Gets or sets the minimum length of time in secodns that an idle session will be maintained.
        /// </summary>
        public int MinIdleTime { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum number of sessions that will be allowed to connect.
        /// </summary>
        public virtual int MaxSessions { get; set; } = 10;

        /// <summary>
        /// Gets or sets the bool allowing the Endpoint's Session Manager to use keepalives during session maintenance.
        /// Note: Keepalives do not reset the Idle timer when received, a session using Keepalives will still timeout.
        /// </summary>
        public bool UseKeepalives { get; set; } = true;

        /// <summary>
        /// Gets or sets the interval that the Endpoint's Session Manager will evaluate the Endpoint's SessionDictionary.
        /// </summary>
        public int SessionManagerIntervalMilliseconds { get; set; } = 1000;
    }
}