using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace DebuggingClient.Classes
{
    internal class Client
    {
        private List<Session> sessionList = new List<Session>();
        int _maxSessionCount = 10;
        private Timer clientTimer;

        public void Intialize()
        {
            clientTimer = new Timer();
            clientTimer.Elapsed += ClientTimerTick;
            clientTimer.Interval = 1000;
            clientTimer.Enabled = true;
        }

        public void ClientTimerTick(object source, ElapsedEventArgs eea)
        {
            lock (sessionList)
            {
                foreach (var session in sessionList)
                {
                    if (session.HelloInterval == 0)
                    {
                        Task.Run(() => session.SendHello());
                        session.HelloInterval = 5;
                    }
                    session.HelloInterval--;
                }
            }
        }

        public void ConsoleCommandProcessor(string command)
        {
            var returnString = "";
            var commandToUpper = command.ToUpper();

            switch (commandToUpper)
            {
                case "SESSIONS":
                {
                        break;
                }
                case "CONNECT":
                {
                    if (sessionList.Count < _maxSessionCount)
                    {
                        Console.Write("Enter server address:port : ");
                        string sessionEndpoint = Console.ReadLine();
                        Session session = new Session(sessionEndpoint);

                        AddSession(session);
                    }
                    break;
                }
                case "CONNECT STRESS":
                {
                    int i = 0;
                    while (i < 10)
                    {
                        Session session = new Session("127.0.0.1:3000");
                        AddSession(session);
                        i++;
                    }
                    break;
                }
            }
        }

        public bool AddSession(Session session)
        {
            if (session == null) throw new ArgumentNullException("session");
            lock (sessionList)
            {
                if (sessionList.Count < _maxSessionCount)
                {
                    sessionList.Add(session);

                    return true;
                }
            }
            return false;
        }

        public void RemoveSession(Session session)
        {
            lock (sessionList)
            {
                if (sessionList.Contains(session))
                {
                    sessionList.Remove(session);
                }
            }
        }
    }
}
