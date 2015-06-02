﻿using System;
using System.Diagnostics;
using System.Net;
using netSharp.Server.Events;

namespace Test.Client
{
    internal class TestClient : IDisposable
    {
        private netSharp.Server.Endpoints.Experimental.Client _client;
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
            Console.WriteLine("Starting up...");
            _client = new netSharp.Server.Endpoints.Experimental.Client();
            _client.SessionCreated += HandleSessionCreatedEvent;
            _client.SessionRemoved += HandleSessionRemovedEvent;
            _client.ServerDataRecieved += HandleServerDataRecieved;
            Console.WriteLine("Started client at {0}", DateTime.Now);
        }

        private void HandleServerDataRecieved(object sender, ServerEvents e)
        {
            Console.WriteLine("New Server Message Recieved");
        }

        private void HandleSessionRemovedEvent(object sender, ServerEvents e)
        {
            Console.WriteLine("Session Removed");
        }

        private void HandleSessionCreatedEvent(object sender, ServerEvents e)
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
                    for (var i = 0; i < 1000; i++)
                    {
                        var remoteIpAddress = IPAddress.Parse("10.0.0.10");
                        var remotePort = 3000;
                        var remoteIpEndpoint = new IPEndPoint(remoteIpAddress, remotePort);
                        _client.NewSession(remoteIpEndpoint);
                    }

                    break;
                }
                case "TEST":
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var test = new byte[10000];
                    _client.SendData(test, "ALLSERVERS");

                    stopwatch.Stop();
                    var ts = stopwatch.Elapsed;

                    Console.WriteLine("{0} ms", ts.TotalMilliseconds);
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