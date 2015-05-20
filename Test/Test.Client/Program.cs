using System;
using System.Net;
using netSharp.TCP.Events;

namespace Test.Client
{
    internal class Program : IDisposable
    {
        private netSharp.TCP.Client _client;
        private bool _isDisposed;

        public void Dispose() //to-do: Call dispose method
        {
            _isDisposed = true;
        }

        private static void Main(string[] args)
        {
            var program = new Program();
            program.Initialize();
            program.InputLoop();
        }

        private void Initialize()
        {
            Console.Write("Starting up...\n\n");
            _client = new netSharp.TCP.Client();
            _client.SessionCreated += HandleSessionCreatedEvent;
            _client.SessionRemoved += HandleSessionRemovedEvent;
            _client.SessionPaused += HandleSessionPausedEvent;
            _client.ServerDataReturn += HandleServerDataReturn;
            _client.ServerMessage += HandleServerMessage;

            Console.Write("Started client at {0}\n\n", DateTime.Now);
        }

        private void HandleServerMessage(object sender, EventDataArgs e)
        {
            Console.WriteLine("New Server Message Recieved");
        }

        private void HandleServerDataReturn(object sender, EventDataArgs e)
        {
            Console.WriteLine("Server Data Returned");
        }

        private void HandleSessionPausedEvent(object sender, EventDataArgs e)
        {
            Console.WriteLine("Session Paused");
        }

        private void HandleSessionRemovedEvent(object sender, EventDataArgs e)
        {
            Console.WriteLine("Session Removed");
        }

        private void HandleSessionCreatedEvent(object sender, EventDataArgs e)
        {
            Console.WriteLine("Session Created");
        }

        private void InputLoop()
        {
            while (!_isDisposed)
            {
                Console.Write("Enter a command :");
                var readLine = Console.ReadLine(); // Wait for console input.
                ConsoleCommandProcessor(readLine);
            }
        }

        public void ConsoleCommandProcessor(string command)
        {
            var commandToUpper = command.ToUpper();

            switch (commandToUpper)
            {
                case "CONNECT":
                {
                    if (_client.SessionList.Count < _client.MaxSessionCount)
                    {
                        var remoteIpAddress = IPAddress.Parse("127.0.0.1");
                        var remotePort = 3000;
                        _client.NewSession(remoteIpAddress, remotePort);
                    }
                    break;
                }
                case "DISCONNECT":
                {
                    foreach (var session in _client.SessionList)
                    {
                        session.Dispose();
                    }
                    break;
                }
                case"TEST":
                {
                    _client.Test();
                    break;
                }
            }
        }
    }
}