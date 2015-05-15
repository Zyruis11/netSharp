using System;

namespace Server
{
    internal class Program
    {
        private Server _server;
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
            _server = new Server(serverBindAddr, serverBindPort);
            Console.Write("Started at {0}\n\n", DateTime.Now);
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
                        _server.StartClientFactory();
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
                        _server.StopClientFactory();
                        Console.Write("Listener stopping. \n");
                    }
                    break;
                }
                case "CLIENTS":
                {
                    //to-do: Show Information on Main
                    Console.Write("\n{0}/{1} Clients connected\n", _server.ClientObjectList.Count, _server._maxClientCount);

                    Console.Write("\nClient Name | Last Heard | Client Address/Port\n\n");

                    lock (_server.ClientObjectList)
                    {
                        foreach (var client in _server.ClientObjectList)
                        {
                            Console.Write("{0}         {1}            {2}\n", client.Guid, client.LastHeard,
                                client.RemoteEndpoint);
                        }
                    }

                    Console.Write("\n{0}/{1} Clients connected\n", _server.ClientObjectList.Count, _server._maxClientCount);

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