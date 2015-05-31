using System;
using System.Net;
using System.Text;
using netSharp.Server.Events;

namespace Test.Server
{
    internal class TestServer
    {
        private netSharp.Server.Objects.Server _server;
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
            var serverBindAddr = "10.0.0.10";
            var serverBindPort = 3000;
            var serverIpEndPoint = new IPEndPoint(IPAddress.Parse(serverBindAddr), serverBindPort);
            _server = new netSharp.Server.Objects.Server(serverIpEndPoint, 20000);
            _server.SessionCreated += HandleSessionCreated;
            _server.SessionRemoved += HandleSessionRemoved;
            _server.ClientDataReceived += HandleSessionDataReceived;
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

        private void HandleSessionDataReceived(object sender, NetSharpEventArgs e)
        {
            //var str = Encoding.Default.GetString(e.DataStream.PayloadByteArray);
            Console.WriteLine("Recieved Data " + e.DataStream.PayloadByteArray.Length);
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
                case "CLIENTS":
                {
                    //to-do: Show Information on Main
                    Console.WriteLine("{0}/{1} Clients connected", _server.SessionList.Count, _server.MaxClientCount);

                    Console.WriteLine("Client GUID | Use Heartbeat? - Idle Time/Max Idle Time | ToClient Address/Port");

                    lock (_server.SessionList)
                    {
                        foreach (var session in _server.SessionList)
                        {
                            Console.WriteLine("{0}          {1} - {2}/{3}                           {4}",
                                session.RemoteEndpointGuid,
                                session.UseHeartbeat, session.IdleTime, session.MaxIdleTime,
                                session.RemoteEndpointIpAddressPort);
                        }
                    }

                    Console.WriteLine("{0}/{1} Clients connected", _server.SessionList.Count, _server.MaxClientCount);
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