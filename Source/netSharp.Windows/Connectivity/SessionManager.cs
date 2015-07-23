using System;
using System.Collections.Generic;

namespace netSharp.Windows.Connectivity
{
    internal class SessionManager
    {

        public SessionManager(netSharpEndpoint endpoint)
        {
            if (endpoint == null)
            {
                throw new NullReferenceException();
            }
            EndpointReference = endpoint;
            MaxIdleTime = 900;
            MinIdleTime = 30;
        }

        public double MaxIdleTime { get; set; }
        public double MinIdleTime { get; set; }
        public netSharpEndpoint EndpointReference { get; set; }
        public bool IsActive { get; set; }

        public void TimerTick()
        {
            IsActive = true;

            if (EndpointReference.SessionList.Count == 0)
            {
                IsActive = false;
                return;
            }

            var sessionsToDispose = new List<Session>();
            
            MaxIdleTime = ScaleMaxIdleTimer(EndpointReference.MaxSessionCount, EndpointReference.SessionList.Count);

            lock (EndpointReference.SessionList)
            {
                foreach (var session in EndpointReference.SessionList)
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
                    IsActive = false;
                    return;
                }

                foreach (var session in sessionsToDispose)
                {
                    EndpointReference.RemoveSession(session);
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();

            IsActive = false;
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