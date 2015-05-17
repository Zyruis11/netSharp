using System.Collections.Generic;

namespace Library.Networking.TCP.Features
{
    internal static class Heartbeat
    {
        public static void Pulse(List<Session> sessionList)
        {
            var clientsToDispose = new List<Session>();

            lock (sessionList)
            {
                foreach (var session in sessionList)
                {
                    session.LastTwoWay += 1;
                    if (session.LastTwoWay >= 30)
                    {
                        clientsToDispose.Add(session);
                        continue;
                    }

                    session.HelloInterval--;
                    if (session.HelloInterval <= 0)
                    {
                        session.SendHeartbeat();
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
    }
}