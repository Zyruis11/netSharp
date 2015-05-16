using System;
using System.Net;
using Library.Networking.TCP;

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
            program._client = new Library.Networking.TCP.Client();
            program.Initialize();
            program.InputLoop();
        }

        private void Initialize()
        {
            _client.Intialize();
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
                            IPAddress remoteIpAddress = IPAddress.Parse("127.0.0.1");
                            int remotePort = 3000;
                            _client.NewSession(remoteIpAddress, remotePort);
                        }
                        break;
                    }
                case "DISCONNECT":
                    {
                        foreach (Session session in _client._sessionList)
                        {
                            session.Dispose();
                        }
                        break;
                    }
            }
        }
    }
}