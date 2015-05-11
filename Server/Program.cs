using System;

namespace Server
{
    internal class Program
    {
        private readonly string serverDisplayName = "Enter a command : ";
        private bool _isDisposed;
        private Objects.Server.Server _server;

        public void Dispose() //to-do: Call dispose method
        {
            _isDisposed = true;
        }

        private static void Main(string[] args)
        {
            var program = new Program();
            program._server = new Objects.Server.Server();
            program.Initialize();
            program.InputLoop();
        }

        private void Initialize()
        {
            Console.Write("Starting up...\n\n");
            var serverBindAddr = "127.0.0.1";
            var serverBindPort = 3000;
            _server.Intialize(serverBindAddr, serverBindPort);
        }

        private void InputLoop()
        {
            while (!_isDisposed)
            {
                Console.Write("Enter a command :");
                var readLine = Console.ReadLine(); // Wait for console input.
                _server.ConsoleCommandProcessor(readLine);
            }
        }

    }
}