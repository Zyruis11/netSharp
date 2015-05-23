using System;
using netSharp.Events;

namespace Test.Server
{
    internal class TestServer
    {
        private netSharp.Objects.Server _server;
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
            _server = new netSharp.Objects.Server(serverBindAddr, serverBindPort, 10, 100);
            _server.ClientCreated += HandleClientCreated;
            _server.ClientRemoved += HandleClientRemoved;
            _server.NewClientRequest += HandleNewClientRequest;
            _server.ListenerPaused += HandleListenerPaused;
            Console.WriteLine("Started server at {0}", DateTime.Now);
        }

        private void HandleClientCreated(object sender, SessionEventArgs e)
        {
            Console.WriteLine("New Client Joined");
        }

        private void HandleClientRemoved(object sender, SessionEventArgs e)
        {
            Console.WriteLine("Client Removed");
        }

        private void HandleNewClientRequest(object sender, SessionEventArgs e)
        {
            Console.WriteLine("New Client Request");
        }

        private void HandleListenerPaused(object sender, SessionEventArgs e)
        {
            Console.WriteLine("Listener Paused");
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
                case "LISTEN":
                {
                    if (!IsDisposed)
                    {
                        Console.WriteLine("Listener already started.");
                    }

                    if (IsDisposed)
                    {
                        _server.StartClientSessionFactory();
                        Console.WriteLine("Listener starting.");
                    }

                    break;
                }
                case "NO LISTEN":
                {
                    if (IsDisposed)
                    {
                        Console.WriteLine("Listener not started.");
                    }

                    if (!IsDisposed)
                    {
                        _server.Dispose();
                        Console.WriteLine("Listener stopping. ");
                    }
                    break;
                }
                case "CLIENTS":
                {
                    //to-do: Show Information on Main
                    Console.WriteLine("{0}/{1} Clients connected", _server.SessionList.Count, _server.MaxClientCount);

                    Console.WriteLine("Client Name | Last Heard | ToClient Address/Port");

                    lock (_server.SessionList)
                    {
                        foreach (var session in _server.SessionList)
                        {
                            Console.WriteLine("{0}         {1}            {2}", session.RemoteEndpointGuid,
                                session.LastHello,
                                session.RemoteEndpointIpAddressPort);
                        }
                    }

                    Console.WriteLine("{0}/{1} Clients connected", _server.SessionList.Count, _server.MaxClientCount);

                    break;
                }
                case "BROADCAST":
                {
                    Console.WriteLine("Type 'exit' to exit the broadcast loop.");
                    while (true)
                    {
                        Console.WriteLine("Enter broadcast text : ");
                        var broadcastString = Console.ReadLine();

                        _server.SendData(broadcastString);

                        if (broadcastString == "exit")
                        {
                            break;
                        }
                    }
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