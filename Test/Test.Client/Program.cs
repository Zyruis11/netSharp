using System;
using System.Net;
using Library.Networking.TCP.Events;

namespace Test.Client
{
    internal class Program : IDisposable
    {
        private Library.Networking.TCP.Client _client;
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
            _client = new Library.Networking.TCP.Client();
            _client.Intialize();
            _client.SessionCreated += HandleNewSessionEvent;
            Console.Write("Started client at {0}\n\n", DateTime.Now);

        }

        private void HandleNewSessionEvent(object sender, TcpEventArgs tcpEventArgs)
        {
            Console.WriteLine(tcpEventArgs.Message);
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
                case "SESSIONS":
                {
                    break;
                }
                case "CONNECT":
                {
                    if (_client._sessionList.Count < _client._maxSessionCount)
                    {
                        var remoteIpAddress = IPAddress.Parse("127.0.0.1");
                        var remotePort = 3000;
                        _client.NewSession(remoteIpAddress, remotePort);
                    }
                    break;
                }
                case "DISCONNECT":
                {
                    foreach (var session in _client._sessionList)
                    {
                        session.Dispose();
                    }
                    break;
                }
            }
        }
    }
}