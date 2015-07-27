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

namespace netSharp.Connectivity
{
    public sealed class SessionManager
    {
        public SessionManager()
        {
            MaxIdleTime = 900;
            MinIdleTime = 30;
        }

        public double MaxIdleTime { get; set; }
        public double MinIdleTime { get; set; }

        public void Manage()
        {
            //TODO: Execute Idle Timer Scaling Algorithim
            //TODO: Check session dictionary for expired sessions or errored sessions
            //TODO: Remove expired or errored sessions
            //TODO: Send keepalives and decrement keepalive values
        }

        //TODO: Improve this a lot.
        private int ScaleMaxIdleTimer(int _maxSessionCount, int _currentSessionCount)
        {
            var usagePercentage = _currentSessionCount/(double) _maxSessionCount;
            var returnValue = (MaxIdleTime - (usagePercentage*MaxIdleTime));

            if (returnValue < MinIdleTime)
            {
                return Convert.ToInt32(MinIdleTime);
            }
            return Convert.ToInt32(returnValue);
        }
    }
}