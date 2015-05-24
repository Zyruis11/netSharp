using System;
using System.Collections.Generic;
using netSharp.Objects;

namespace netSharp.Components
{
    internal static class SessionManager
    {
        public static void SessionStateEngine(List<Session> sessionList)
        {
            var sessionsToDispose = new List<Session>();

            lock (sessionList)
            {
                foreach (var session in sessionList)
                {
                    session.IdleTimer += 1;
                    if (session.IdleTimer >= 300)
                    {
                        sessionsToDispose.Add(session);
                        continue;
                    }

                    session.TimeSinceLastHeartbeatRecieve += 1;
                    if (session.TimeSinceLastHeartbeatRecieve >= 30)
                    {
                        sessionsToDispose.Add(session);
                        continue;
                    }

                    session.TimeUntilNextHeartbeatSend--;
                    if (session.TimeUntilNextHeartbeatSend <= 0)
                    {
                        session.SendData(CreateHelloStream(session.LocalEndpointGuid));
                        session.TimeUntilNextHeartbeatSend = 10;
                    }
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

        public static void ProcessRecievedHello(Session session)
        {
            session.TimeSinceLastHeartbeatRecieve = 0;
        }

        public static DataStream CreateHelloStream(string localEndpointGuid)
        {
            var dataStream = new DataStream(localEndpointGuid, 0, "");
            return dataStream;
        }
    }
}