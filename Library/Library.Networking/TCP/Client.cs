using System;
using System.Collections.Generic;
using System.Timers;

namespace Library.Networking.TCP
{
    public class Client : IDisposable
    {
        public readonly int _maxSessionCount = 10;
        public readonly List<ServerSession> _sessionList = new List<ServerSession>();
        private Timer _clientTimer;
        private bool _isDisposed;
        public string Guid;

        public void Dispose()
        {
            _isDisposed = true;
        }

        public void Intialize()
        {
            Console.Write("Started at {0}\n\n", DateTime.Now);
            Guid = Convert.ToString(System.Guid.NewGuid()).Remove(5);
            _clientTimer = new Timer();
            _clientTimer.Elapsed += ClientTimerTick;
            _clientTimer.Interval = 1000;
            _clientTimer.Enabled = true;
        }

        public void ClientTimerTick(object source, ElapsedEventArgs eea)
        {
            var sessionsToDispose = new List<ServerSession>();

            lock (_sessionList)
            {
                foreach (var session in _sessionList)
                {
                    session.LastHeard += 1;
                    session.HelloInterval--;

                    if (session.LastHeard >= 30)
                    {
                        sessionsToDispose.Add(session);
                    }
                    else if (session.HelloInterval == 0)
                    {
                        session.SendKeepaliveHello();
                        session.HelloInterval = 10;
                    }
                }

                foreach (var session in sessionsToDispose)
                {
                    session.Dispose();
                    _sessionList.Remove(session);
                    Console.Write("ServerSession disposed.");
                }
            }
        }

        public bool AddSession(ServerSession serverSession)
        {
            if (serverSession == null) throw new ArgumentNullException("serverSession");
            lock (_sessionList)
            {
                if (_sessionList.Count < _maxSessionCount)
                {
                    _sessionList.Add(serverSession);

                    return true;
                }
            }
            return false;
        }

        public void RemoveSession(ServerSession serverSession)
        {
            lock (_sessionList)
            {
                if (_sessionList.Contains(serverSession))
                {
                    _sessionList.Remove(serverSession);
                }
            }
        }
    }
}