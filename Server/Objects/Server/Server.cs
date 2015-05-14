using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using Server.Objects.Server.Request;

namespace Server.Objects.Server
{
    internal class Server : IDisposable
    {
        private readonly List<Client.Client> clientList = new List<Client.Client>();
        private Timer _clientTimer;
        private bool _isDisposed;
        private int _logicalCoreCount;
        private int _maxClientCount;
        private ServerWorker _serverWorker;
        public bool IsInitialized;
        public int MaxClientRequests;
        public RequestBroker RequestBrokerObj;

        public void Dispose()
        {
            _isDisposed = true;
        }

        public void Intialize(string serverBindAddr, int serverBindPort)
        {
            Console.Write("Started at {0}\n\n", DateTime.Now);

            CheckEnvironment();

            Console.Write(
                "{0} logical processors available.\n{1} client(s) supported.\n{2} concurrent requests supported.\n\n",
                _logicalCoreCount, _maxClientCount, MaxClientRequests);
            Console.Write("Bind Address ({0})\n", serverBindAddr);
            Console.Write("Bind Port ({0})\n\n", serverBindPort);

            _serverWorker = new ServerWorker(this, IPAddress.Parse(serverBindAddr), serverBindPort);
            _serverWorker.StartListener();
            IsInitialized = true;

            _clientTimer = new Timer();
            _clientTimer.Elapsed += ClientTimerTick;
            _clientTimer.Interval = 1000;
            _clientTimer.Enabled = true;

            // ReSharper disable once InconsistentlySynchronizedField
            RequestBrokerObj = new RequestBroker(MaxClientRequests);
        }

        private void ClientTimerTick(object source, ElapsedEventArgs eea)
        {
            var clientsToDispose = new List<Client.Client>();

            lock (clientList)
            {
                foreach (var client in clientList)
                {
                    client.LastHeard += 1;

                    if (client.LastHeard >= 20)
                    {
                        clientsToDispose.Add(client);
                    }
                }

                foreach (var client in clientsToDispose)
                {
                    client.Dispose();
                    clientList.Remove(client);
                    Console.WriteLine("\nClient {0} timed out and was disposed.", client.Guid);
                }
            }
        }

        public void ConsoleCommandProcessor(string command)
        {
            var commandToUpper = command.ToUpper();

            switch (commandToUpper)
            {
                case "LISTEN":
                {
                    if (!_serverWorker.IsDisposed)
                    {
                        Console.Write("Listener already started.\n");
                    }

                    if (_serverWorker.IsDisposed)
                    {
                        _serverWorker.StartListener();
                        Console.Write("Listener starting. \n");
                    }

                    break;
                }
                case "NO LISTEN":
                {
                    if (_serverWorker.IsDisposed)
                    {
                        Console.Write("Listener not started.\n");
                    }

                    if (!_serverWorker.IsDisposed)
                    {
                        _serverWorker.StopListener();
                        Console.Write("Listener stopping. \n");
                    }
                    break;
                }
                case "CLIENTS":
                {
                    //to-do: Show Information on Main
                    Console.Write("\n{0}/{1} Clients connected\n", clientList.Count, _maxClientCount);

                    Console.Write("\nClient Name | Last Heard | Client Address/Port\n\n");

                    lock (clientList)
                    {
                        foreach (var client in clientList)
                        {
                            Console.Write("{0}         {1}            {2}\n", client.Guid, client.LastHeard,
                                client.RemoteEp);
                        }
                    }

                    Console.Write("\n{0}/{1} Clients connected\n", clientList.Count, _maxClientCount);

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

                        ClientBroadcast(broadcastString);
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

        public void ClientBroadcast(string broadcastString)
        {
            lock (clientList)
            {
                foreach (var client in clientList)
                {
                    client.SendString(broadcastString);
                }
            }
        }

        public bool AddClient(Client.Client client)
        {
            lock (clientList)
            {
                if (clientList.Count < _maxClientCount)
                {
                    clientList.Add(client);

                    return true;
                }
            }
            return false;
        }

        public void RemoveClient(Client.Client client)
        {
            lock (clientList)
            {
                if (clientList.Contains(client))
                {
                    clientList.Remove(client);
                }
            }
        }

        public bool AddClientRequest(string guid)
        {
            bool returnBool;
            lock (RequestBrokerObj)
            {
                returnBool = RequestBrokerObj.AddRequest(guid);
            }
            return returnBool;
        }

        public bool RemoveClientRequest(string guid)
        {
            bool returnBool;
            lock (RequestBrokerObj)
            {
                returnBool = RequestBrokerObj.RemoveRequest(guid);
            }
            return returnBool;
        }

        private void CheckEnvironment()
        {
            _logicalCoreCount = Environment.ProcessorCount;
            _maxClientCount = _logicalCoreCount*5;
            MaxClientRequests = _logicalCoreCount*2;
        }
    }
}