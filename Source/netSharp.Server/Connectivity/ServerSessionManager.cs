using System;
using System.Collections.Generic;
using netSharp.Core.Data;

namespace netSharp.Server.Connectivity
{
    internal static class ServerSessionManager
    {
        public static void SessionStateEngine(List<ServerSession> sessionList, int maxSessionCount = 0)
        {
            if (sessionList.Count == 0)
            {
                return;
            }

            var sessionsToDispose = new List<ServerSession>();
            var sessionsToInitialize = new List<ServerSession>();
            var maxIdleTime = 900;
            var sessionsToInitializeCount = 250; // Max number of sessions to initialize per pass

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

                    if (!session.SentGuid && sessionsToInitializeCount > 0)
                    {
                        sessionsToInitializeCount--;
                        sessionsToInitialize.Add(session);
                    }                   
                }

                if (sessionsToInitialize.Count > 0)
                {
                    SessionIntializer(sessionsToInitialize);
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

        public static void SessionIntializer(List<ServerSession> sessionsToInitialize)
        {
            foreach (var session in sessionsToInitialize)
            {
                if (session.IsDisposed != true)
                {
                    session.StreamWriterAsync(CreateHelloStream(session.LocalEndpointGuid));
                    session.SentGuid = true;
                }
            }
        }

        public static DataStream CreateHelloStream(string localEndpointGuid)
        {
            byte[] helloBytes = new byte[0];
            var dataStream = new DataStream(localEndpointGuid, 0, helloBytes);
            return dataStream;
        }
    }
}