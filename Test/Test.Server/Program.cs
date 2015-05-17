using System;

namespace Test.Server
{
    internal class Program
    {
        private Library.Networking.TCP.Server _server;
        private bool IsDisposed;

        public void Dispose() //to-do: Call dispose method
        {
            IsDisposed = true;
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
            var serverBindAddr = "127.0.0.1";
            var serverBindPort = 3000;
            _server = new Library.Networking.TCP.Server(serverBindAddr, serverBindPort, 10, 100);
            Console.Write("Started server at {0}\n\n", DateTime.Now);
        }

        private void InputLoop()
        {
            while (!IsDisposed)
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
                case "LISTEN":
                {
                    if (!IsDisposed)
                    {
                        Console.Write("Listener already started.\n");
                    }

                    if (IsDisposed)
                    {
                        _server.StartClientSessionFactory();
                        Console.Write("Listener starting. \n");
                    }

                    break;
                }
                case "NO LISTEN":
                {
                    if (IsDisposed)
                    {
                        Console.Write("Listener not started.\n");
                    }

                    if (!IsDisposed)
                    {
                        _server.Dispose();
                        Console.Write("Listener stopping. \n");
                    }
                    break;
                }
                case "CLIENTS":
                {
                    //to-do: Show Information on Main
                    Console.Write("\n{0}/{1} Clients connected\n", _server.SessionList.Count, _server.MaxClientCount);

                    Console.Write("\nClient Name | Last Heard | ToClient Address/Port\n\n");

                    lock (_server.SessionList)
                    {
                        foreach (var session in _server.SessionList)
                        {
                            Console.Write("{0}         {1}            {2}\n", session.GetFriendlyEndpointGuid(),
                                session.LastTwoWay,
                                session.RemoteEndpointIpAddressPort);
                        }
                    }

                    Console.Write("\n{0}/{1} Clients connected\n", _server.SessionList.Count, _server.MaxClientCount);

                    break;
                }
                case "BROADCAST":
                {
                    Console.Write("Type 'exit' to exit the broadcast loop.\n");
                    while (true)
                    {
                        Console.Write("\nEnter broadcast text : ");
                        var broadcastString = Console.ReadLine();

                        if (broadcastString == "exit")
                        {
                            break;
                        }

                        _server.ClientBroadcast(broadcastString);
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
                    Console.Write("Command {0} is invalid.\n", command);
                    break;
                }
            }
        }
    }
}