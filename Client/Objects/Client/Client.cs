using System;
using System.Collections.Generic;
using System.Timers;

namespace Client.Objects.Client
{
    internal class Client : IDisposable
    {
        private readonly int _maxSessionCount = 10;
        private readonly List<Session.Session> _sessionList = new List<Session.Session>();
        private Timer _clientTimer;
        private bool _isDisposed;
        private string Guid;

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
            var sessionsToDispose = new List<Session.Session>();

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
                        session._sessionWorker.SendKeepaliveHello();
                        session.HelloInterval = 10;
                    }
                }

                foreach (var session in sessionsToDispose)
                {
                    session.Dispose();
                    _sessionList.Remove(session);
                    Console.Write("Session disposed.");
                }
            }
        }

        public void ConsoleCommandProcessor(string command)
        {
            var commandToUpper = command.ToUpper();

            switch (commandToUpper)
            {
                case "SESSIONS":
                {
                    break;
                }
                case "CONNECT":
                {
                    if (_sessionList.Count < _maxSessionCount)
                    {
                        //Console.Write("Enter server address:port : ");
                        //var sessionEndpoint = Console.ReadLine();
                        var session = new Session.Session("127.0.0.1:3000", Guid);

                        AddSession(session);
                    }
                    break;
                }
                case "DISCONNECT":
                {
                    foreach (Session.Session session in _sessionList)
                    {
                        session.Dispose();
                    }
                    break;
                }
            }
        }

        public bool AddSession(Session.Session session)
        {
            if (session == null) throw new ArgumentNullException("session");
            lock (_sessionList)
            {
                if (_sessionList.Count < _maxSessionCount)
                {
                    _sessionList.Add(session);

                    return true;
                }
            }
            return false;
        }

        public void RemoveSession(Session.Session session)
        {
            lock (_sessionList)
            {
                if (_sessionList.Contains(session))
                {
                    _sessionList.Remove(session);
                }
            }
        }
    }
}