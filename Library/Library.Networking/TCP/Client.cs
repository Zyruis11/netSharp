using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using Library.Networking.TCP.Features;

namespace Library.Networking.TCP
{
    public class Client : IDisposable
    {
        public readonly int _maxSessionCount = 10;
        public readonly List<Session> _sessionList = new List<Session>();
        private Timer _clientTimer;
        private bool _isDisposed;
        public Guid ClientGuid;

        public void Dispose()
        {
            _isDisposed = true;
        }

        public void Intialize()
        {
            Console.Write("Started at {0}\n\n", DateTime.Now);
            
            ClientGuid = Guid.NewGuid();

            _clientTimer = new Timer(1000);
            _clientTimer.Elapsed += ClientTimerTick;
            _clientTimer.Enabled = true;
        }

        public void ClientTimerTick(object source, ElapsedEventArgs eea)
        {
            Heartbeat.Pulse(_sessionList);
        }

        public void NewSession(IPAddress remoteIpAddress, int remotePort)
        {
            var remoteEndpoint = new IPEndPoint(remoteIpAddress, remotePort);
            Session session = new Session(1, remoteEndpoint, ClientGuid);
            AddSession(session);
        }

        public bool AddSession(Session serverSession)
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

        public void RemoveSession(Session serverSession)
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