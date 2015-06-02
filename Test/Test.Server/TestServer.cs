using System;
using System.Net;
using System.Text;
using netSharp.Server.Events;

namespace Test.Server
{
    internal class TestServer
    {
        private netSharp.Server.Endpoints.Server _server;
        private bool IsDisposed;

        public void Dispose() //to-do: Call dispose method
        {
            IsDisposed = true;
        }

        private static void Main(string[] args)
        {
            var program = new TestServer();
            program.Initialize();
            program.InputLoop();
        }

        private void Initialize()
        {
            Console.WriteLine("Starting up...");
            var serverBindAddr = "127.0.0.1";
            var serverBindPort = 3000;
            var serverIpEndPoint = new IPEndPoint(IPAddress.Parse(serverBindAddr), serverBindPort);
            _server = new netSharp.Server.Endpoints.Server(serverIpEndPoint, 20000);
            _server.SessionCreated += HandleSessionCreated;
            _server.SessionRemoved += HandleSessionRemoved;
            _server.ClientDataReceived += HandleSessionDataReceived;
            Console.WriteLine("Started server at {0}", DateTime.Now);
        }

        private void HandleSessionCreated(object sender, ServerEvents e)
        {
            Console.WriteLine("New Client Joined");
        }

        private void HandleSessionRemoved(object sender, ServerEvents e)
        {
            Console.WriteLine("Client Removed");
        }

        private void HandleSessionDataReceived(object sender, ServerEvents e)
        {
            Console.WriteLine("Session Data Received");
        }

        private void InputLoop()
        {
            while (!IsDisposed)
            {
                Console.WriteLine("Enter a command :");
                var readLine = Console.ReadLine(); // Wait for console input.
                ConsoleCommandProcessor(readLine);
            }
        }

        public void ConsoleCommandProcessor(string command)
        {
            var commandToUpper = command.ToUpper();

            switch (commandToUpper)
            {
                case "SEND DATA":
                {
                    //TODO: Specify the recipient
                    byte[] data = new byte[1000];
                    _server.SendDataAsync(data);
                    break;
                }
                case "START LISTENER":
                {
                    _server.StartListener();
                    Console.WriteLine("Listener Started");
                    break;
                }

                case "STOP LISTENER":
                {
                    _server.StopListener();
                    Console.WriteLine("Listener Stopped");
                    break;
                }
                case "ADD SESSION":
                {
                    //TODO: Get parameters and create session
                    break;
                }
                case "REMOVE SESSION":
                {
                    //TODO: Find the session to be removed
                    //_server.RemoveSession(session);
                    break;
                }
                case "CLEAR":
                {
                    Console.Clear();
                    break;
                }
                default:
                {
                    Console.WriteLine("Command {0} is invalid.\n", command);
                    break;
                }
            }
        }
    }
}