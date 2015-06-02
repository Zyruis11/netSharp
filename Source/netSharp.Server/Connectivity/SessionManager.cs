using System;
using System.Collections.Generic;
using netSharp.Core.Data;

namespace netSharp.Server.Connectivity
{
    internal static class SessionManager
    {
        public static void SessionStateEngine(List<Session> sessionList, int maxSessionCount = 0)
        {
            if (sessionList.Count == 0)
            {
                return;
            }

            var sessionsToDispose = new List<Session>();
            var maxIdleTime = 900;

            if (maxSessionCount != 0 && sessionList.Count != 0)
            {
                maxIdleTime = ScaleMaxIdleTimer(maxSessionCount, sessionList.Count);
            }

            lock (sessionList)
            {
                foreach (var session in sessionList)
                {
                    session.IdleTime += 1;
                    session.MaxIdleTime = maxIdleTime;
                    if (session.IdleTime >= maxIdleTime)
                    {
                        sessionsToDispose.Add(session);
                        continue;
                    }              
                }             

                if (sessionsToDispose.Count == 0)
                {
                    return;
                }

                foreach (var session in sessionsToDispose)
                {
                    sessionList.Remove(session);
                    session.Dispose();                   
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private static int ScaleMaxIdleTimer(int maxSessionCount, int currentSessionCount)
        {
            var baseIdleTimer = 900.0;
            var minIdleTimer = 30.0;

            var usagePercentage = currentSessionCount/(double) maxSessionCount;

            var returnValue = (baseIdleTimer - (usagePercentage*baseIdleTimer));

            if (returnValue < minIdleTimer)
            {
                return Convert.ToInt32(minIdleTimer);
            }
            return Convert.ToInt32(returnValue);
        }

        public static DataStream CreateHelloStream(string localEndpointGuid)
        {
            byte[] helloBytes = new byte[0];
            var dataStream = new DataStream(localEndpointGuid, 0, helloBytes);
            return dataStream;
        }
    }
}