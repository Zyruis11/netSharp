using System;
using System.Diagnostics;
using System.Net;
using netSharp.Server.Connectivity;
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
            Console.WriteLine("Session Data Recieved");
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
                case "ADD SESSION":
                {
                    try
                    {
                        IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Loopback, 3000);
                        Session session = new Session(ipEndPoint);
                        _client.SessionList.Add(session);
                    }
                    catch
                    {
                        Console.WriteLine("Unable to add session");
                    }
                    break;
                }
                case "REMOVE SESSION":
                {
                    //TODO: Find the session to be removed
                    //_client.SessionList.Remove(session);
                    break;
                }
                case "SEND DATA":
                {
                    //TODO: Find the session to send data to
                    byte[] data = new byte[1000];
                    _client.SendDataAsync(data, null);
                    break;
                }
                case "SHOW GUID":
                {
                    Console.WriteLine(_client.ClientGuid);
                    break;
                }
                case "CLEAR":
                {
                    Console.Clear();
                    break;
                }
            }
        }
    }
}