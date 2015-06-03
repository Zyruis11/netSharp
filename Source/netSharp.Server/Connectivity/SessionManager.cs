using System;
using System.Collections.Generic;

namespace netSharp.Server.Connectivity
{
    internal class SessionManager
    {

        public SessionManager(NsEndpoint endpoint)
        {
            if (endpoint == null)
            {
                throw new NullReferenceException();
            }
            EndpointContext = endpoint;
            MaxIdleTime = 900;
            MinIdleTime = 30;
        }

        public double MaxIdleTime { get; set; }
        public double MinIdleTime { get; set; }
        public NsEndpoint EndpointContext { get; set; }

        public void TimerTick()
        {
            if (EndpointContext.SessionList.Count == 0)
            {
                return;
            }

            var sessionsToDispose = new List<Session>();
            
            MaxIdleTime = ScaleMaxIdleTimer(EndpointContext.MaxSessionCount, EndpointContext.SessionList.Count);
            
            lock (EndpointContext.SessionList)
            {
                foreach (var session in EndpointContext.SessionList)
                {
                    session.IdleTime += 1;
                    session.MaxIdleTime = MaxIdleTime;
                    if (session.IdleTime >= MaxIdleTime)
                    {
                        sessionsToDispose.Add(session);
                    }
                }

                if (sessionsToDispose.Count == 0)
                {
                    return;
                }

                foreach (var session in sessionsToDispose)
                {
                    EndpointContext.SessionList.Remove(session);
                    session.Dispose();
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private int ScaleMaxIdleTimer(int maxSessionCount, int currentSessionCount)
        {
            var usagePercentage = currentSessionCount/(double) maxSessionCount;
            var returnValue = (MaxIdleTime - (usagePercentage*MaxIdleTime));

            if (returnValue < MinIdleTime)
            {
                return Convert.ToInt32(MinIdleTime);
            }
            return Convert.ToInt32(returnValue);
        }
    }
}