using System;
using System.Collections.Generic;
using netSharp.Endpoint;

namespace netSharp.Sessions
{
    internal class SessionManager
    {

        public SessionManager()
        {
            MaxIdleTime = 900;
            MinIdleTime = 30;
        }

        public double MaxIdleTime { get; set; }
        public double MinIdleTime { get; set; }
        public BaseEndpoint EndpointReference { get; set; }
        public bool IsActive { get; set; }

        public void TimerTick()
        {
            IsActive = true;

            if (EndpointReference.SessionDictionary.Count == 0)
            {
                IsActive = false;
                return;
            }

            var sessionsToDispose = new List<Session>();
            
            MaxIdleTime = ScaleMaxIdleTimer(EndpointReference.MaxSessionCount, EndpointReference.SessionDictionary.Count);

            lock (EndpointReference.SessionDictionary)
            {
                foreach (var session in EndpointReference.SessionDictionary)
                {
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