using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Networking.TCP.Features
{
   static class Heartbeat
    {
        public static void Pulse(List<Session> _sessionList )
        {
            var clientsToDispose = new List<Session>();

            lock (_sessionList)
            {
                foreach (var session in _sessionList)
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
                    _sessionList.Remove(client);
                }
            }
        }
    }
}
