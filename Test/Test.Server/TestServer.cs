using System;
using System.Net;
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

            IPEndPoint serverIpEndPoint = new IPEndPoint(IPAddress.Parse(serverBindAddr), serverBindPort);

            _server = new netSharp.Objects.Server(serverIpEndPoint, 10000);
            _server.SessionCreated += HandleSessionCreated;
            _server.SessionRemoved += HandleSessionRemoved;
            _server.ClientDataReceived += HandleClientData;
            Console.WriteLine("Started server at {0}", DateTime.Now);
        }

        private void HandleSessionCreated(object sender, NetSharpEventArgs e)
        {
            Console.WriteLine("New Client Joined");
        }

        private void HandleSessionRemoved(object sender, NetSharpEventArgs e)
        {
            Console.WriteLine("Client Removed");
        }

        private void HandleClientData(object sender, NetSharpEventArgs e)
        {
            Console.Write("!");    
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

                    Console.WriteLine("Client GUID | Use Heartbeat? - Idle Time/Max Idle Time | ToClient Address/Port");

                    lock (_server.SessionList)
                    {
                        foreach (var session in _server.SessionList)
                        {
                            Console.WriteLine("{0}          {1} - {2}/{3}                           {4}", session.RemoteEndpointGuid,
                                session.UseHeartbeat, session.IdleTime, session.MaxIdleTime,
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