using System;
using TcpIpServer.Classes;

namespace TcpIpServer
{
    internal class Program
    {
        private readonly string serverDisplayName = "Enter a command : ";
        private bool _isDisposed;
        private Server _server;

        private static void Main(string[] args)
        {
            var program = new Program();
            // Declare an instance of the program class to make it's non-static methods 
            program._server = new Server();
            program.InputLoop();
        }

        private void InputLoop()
        {
            Console.Write("Starting up...\n\n");

            var serverBindAddr = "127.0.0.1";
            var serverBindPort = 3000;     

            _server.Intialize(serverBindAddr, serverBindPort);

            while (!_isDisposed)
            {
                Console.Write(serverDisplayName);
                var readLine = Console.ReadLine(); // Wait for console input.
                Console.Write(_server.ConsoleCommandProcessor(readLine) + "\n");
            }
        }
    }
}