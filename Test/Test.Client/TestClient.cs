﻿using System;
using System.Net;
using netSharp.Events;

namespace Test.Client
{
    internal class TestClient : IDisposable
    {
        private netSharp.Objects.Client _client;
        private bool _isDisposed;

        public void Dispose() //to-do: Call dispose method
        {
            _isDisposed = true;
        }

        private static void Main(string[] args)
        {
            var program = new TestClient();
            program.Initialize();
            program.InputLoop();
        }

        private void Initialize()
        {
            Console.Write("Starting up...\n\n");
            _client = new netSharp.Objects.Client();
            _client.SessionCreated += HandleSessionCreatedEvent;
            _client.SessionRemoved += HandleSessionRemovedEvent;
            _client.SessionPaused += HandleSessionPausedEvent;
            _client.ServerDataReturn += HandleServerDataReturn;
            _client.ServerMessage += HandleServerMessage;

            Console.Write("Started client at {0}\n\n", DateTime.Now);
        }

        private void HandleServerMessage(object sender, SessionEventArgs e)
        {
            Console.WriteLine("New Server Message Recieved");
        }

        private void HandleServerDataReturn(object sender, SessionEventArgs e)
        {
            Console.WriteLine("Server Data Returned");
        }

        private void HandleSessionPausedEvent(object sender, SessionEventArgs e)
        {
            Console.WriteLine("Session Paused");
        }

        private void HandleSessionRemovedEvent(object sender, SessionEventArgs e)
        {
            Console.WriteLine("Session Removed");
        }

        private void HandleSessionCreatedEvent(object sender, SessionEventArgs e)
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
                case "TEST":
                {
                    string test = "Hello World!";
                    _client.SendData(test);
                    break;
                }
                case "CLIENTS":
                {
                    //to-do: Show Information on Main
                    Console.WriteLine("{0} Server sessions", _client.SessionList.Count);

                    Console.WriteLine("Server Name | Last Heard | Server Address/Port");

                    lock (_client.SessionList)
                    {
                        foreach (var session in _client.SessionList)
                        {
                            Console.WriteLine("{0}         {1}            {2}", session.RemoteEndpointGuid,
                                session.LastHello,
                                session.RemoteEndpointIpAddressPort);
                        }
                    }

                    Console.WriteLine("{0} Clients connected", _client.SessionList.Count);

                    break;
                }
                case "SHOW GUID":
                {
                    Console.WriteLine(_client.ClientGuid);
                    break;
                }
            }
        }
    }
}