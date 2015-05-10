using System;
using System.Threading.Tasks;
using DebuggingClient.Classes;

namespace DebuggingClient
{
    internal class Program : IDisposable
    {
        private Client _client;
        private bool _isDisposed;

        public void Dispose()
        {
            _isDisposed = true;
        }

        private static void Main(string[] args)
        {
            var program = new Program();
            program._client = new Client();
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
                _client.ConsoleCommandProcessor(readLine);
            }
        }
    }
}