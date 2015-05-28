using System;
using System.Collections.Generic;
using netSharp.Objects;

namespace netSharp.Components
{
    internal static class SessionManager
    {
        public static void SessionStateEngine(List<Session> sessionList, int maxSessionCount = 0)
        {
            var sessionsToDispose = new List<Session>();
            var sessionsToInitialize = new List<Session>();
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

                    //if (!session.SentGuid)
                    //{
                    //    sessionsToInitialize.Add(session);
                    //}

                    if (!session.UseHeartbeat)
                    {
                        continue;
                    }

                    session.TimeSinceLastHeartbeatRecieve += 1;
                    if (session.TimeSinceLastHeartbeatRecieve >= session.MaxTimeSinceLastHeartbeatReceive)
                    {
                        sessionsToDispose.Add(session);
                        continue;
                    }

                    session.TimeUntilNextHeartbeatSend -= 1;
                    if (session.TimeUntilNextHeartbeatSend <= 0)
                    {
                        session.SendData(CreateHelloStream(session.LocalEndpointGuid));
                        session.TimeUntilNextHeartbeatSend = 10;
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
                    session.Dispose();
                    sessionList.Remove(session);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
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

        public static void ProcessRecievedHello(Session session)
        {
            session.TimeSinceLastHeartbeatRecieve = 0;
        }

        public static async void SessionIntializer(List<Session> sessionsToInitialize)
        {
            foreach (var session in sessionsToInitialize)
            {
                session.SendData(CreateHelloStream(session.LocalEndpointGuid));
                session.SentGuid = true;
            }
        }

        public static DataStream CreateHelloStream(string localEndpointGuid)
        {
            var dataStream = new DataStream(localEndpointGuid, 0, "");
            return dataStream;
        }
    }
}