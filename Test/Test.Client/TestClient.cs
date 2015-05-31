using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using netSharp.Server.Events;

namespace Test.Client
{
    internal class TestClient : IDisposable
    {
        private netSharp.Server.Objects.Client _client;
        private bool _isDisposed;

        public void Dispose() //to-do: Call dispose method
        {
            _isDisposed = true;
        }

        private static void Main(string[] args)
        {
            var program = new TestClient();
            program.Initialize();
            program.InputLoop();
        }

        private void Initialize()
        {
            Console.WriteLine("Starting up...");
            _client = new netSharp.Server.Objects.Client();
            _client.SessionCreated += HandleSessionCreatedEvent;
            _client.SessionRemoved += HandleSessionRemovedEvent;
            _client.ServerDataRecieved += HandleServerDataRecieved;
            _client.SessionError += HandleSessionErrorEvent;

            Console.WriteLine("Started client at {0}", DateTime.Now);
        }

        private void HandleServerDataRecieved(object sender, NetSharpEventArgs e)
        {
            Console.WriteLine("New Server Message Recieved");
        }

        private void HandleSessionRemovedEvent(object sender, NetSharpEventArgs e)
        {
            Console.WriteLine("Session Removed");
        }

        private void HandleSessionCreatedEvent(object sender, NetSharpEventArgs e)
        {
            Console.WriteLine("Session Created");
        }

        private void HandleSessionErrorEvent(object sender, NetSharpEventArgs e)
        {
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
                case "CONNECT":
                {
                    for (int i = 0; i < 5000; i++)
                    {
                        var remoteIpAddress = IPAddress.Parse("10.0.0.10");
                        var remotePort = 3000;
                        var remoteIpEndpoint = new IPEndPoint(remoteIpAddress, remotePort);
                        _client.NewSession(remoteIpEndpoint);
                    }

                    break;
                }
                case "DISCONNECT":
                {
                    foreach (var session in _client.SessionList)
                    {
                        session.Dispose();
                    }
                    break;
                }
                case "TEST":
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    string sendString = "Hello World!!!";

                    //var test = Encoding.Default.GetBytes(sendString);
                    var test = new byte[10000];
                     _client.SendData(test, "ALLSERVERS");

                    stopwatch.Stop();
                    TimeSpan ts = stopwatch.Elapsed;

                    Console.WriteLine("{0} ms", ts.TotalMilliseconds);
                    break;
                }
                case "CLIENTS":
                {
                    //to-do: Show Information on Main
                    Console.WriteLine("{0} Server sessions", _client.SessionList.Count);

                    Console.WriteLine("Server Name | Last Heard | Server Address/Port");

                    lock (_client.SessionList)
                    {
                        foreach (var session in _client.SessionList)
                        {
                            Console.WriteLine("{0}         {1}            {2}", session.RemoteEndpointGuid,
                                session.TimeSinceLastHeartbeatRecieve,
                                session.RemoteEndpointIpAddressPort);
                        }
                    }

                    Console.WriteLine("{0} Clients connected", _client.SessionList.Count);

                    break;
                }
                case "SHOW GUID":
                {
                    Console.WriteLine(_client.ClientGuid);
                    break;
                }
            }
        }
    }
}