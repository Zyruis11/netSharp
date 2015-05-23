using System.Collections.Generic;
using netSharp.Objects;

namespace netSharp.Components
{
    internal static class Keepalive
    {
        public static void SessionManager(List<Session> sessionList)
        {
            var clientsToDispose = new List<Session>();

            lock (sessionList)
            {
                foreach (var session in sessionList)
                {
                    session.LastHello += 1;
                    if (session.LastHello >= 30)
                    {
                        clientsToDispose.Add(session);
                        continue;
                    }

                    session.HelloInterval--;
                    if (session.HelloInterval <= 0)
                    {
                        session.SendHello();
                        session.HelloInterval = 10;
                    }
                }

                foreach (var client in clientsToDispose)
                {
                    client.Dispose();
                    sessionList.Remove(client);
                }
            }
        }

        public static void ProcessRecievedHello(Session session)
        {
            session.LastHello = 0;
        }

        public static DataStream CreateHelloStream(string localEndpointGuid)
        {
            DataStream dataStream = new DataStream(localEndpointGuid, 0, "");
            return dataStream;
        }
    }
}