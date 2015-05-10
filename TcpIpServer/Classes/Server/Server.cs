using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;

namespace TcpIpServer.Classes
{
    internal class Server
    {
        private readonly List<Client> clientList = new List<Client>();
        private int _currentClientCount;
        private int _currentRequestLength;
        private bool _isDisposed;
        private bool _isListening;
        private int _logicalCoreCount;
        private int _maxClientCount;
        public int _maxClientRequests;
        private Timer clientTimer;
        public bool IsInitialized;
        public RequestBroker requestBroker;
        private ServerWorker serverListener;

        public void Intialize(string serverBindAddr, int serverBindPort)
        {
            Console.Write("Checking environment...\n\n");

            CheckEnvironment();

            Console.Write(
                "{0} logical processors available.\n{1} client(s) supported.\n{2} concurrent requests supported.\n\n",
                _logicalCoreCount, _maxClientCount, _maxClientRequests);
            Console.Write("Bind Address ({0})\n", serverBindAddr);
            Console.Write("Bind Port ({0})\n\n", serverBindPort);

            serverListener = new ServerWorker(this, IPAddress.Parse(serverBindAddr), serverBindPort);
            serverListener.StartListener();
            IsInitialized = true;

            clientTimer = new Timer();
            clientTimer.Elapsed += ClientTimerTick;
            clientTimer.Interval = 1000;
            clientTimer.Enabled = true;

            requestBroker = new RequestBroker(_maxClientRequests);

            Console.Write("Started at {0}\n", DateTime.Now);
        }

        public string ConsoleCommandProcessor(string command)
        {
            var returnString = "";
            var commandToUpper = command.ToUpper();

            switch (commandToUpper)
            {
                case "LISTEN":
                {
                    if (!serverListener.IsDisposed)
                    {
                        returnString = "Listener already started.\n";
                    }

                    if (serverListener.IsDisposed)
                    {
                        serverListener.StartListener();
                        returnString = "Listener starting. \n";
                    }

                    break;
                }
                case "NO LISTEN":
                {
                    if (serverListener.IsDisposed)
                    {
                        returnString = "Listener not started.\n";
                    }

                    if (!serverListener.IsDisposed)
                    {
                        serverListener.StopListener();
                        returnString = "Listener stopping. \n";
                    }
                    break;
                }
                case "CLIENTS":
                {
                    //to-do: Show Information on Main
                    returnString = string.Format("\n{0}/{1} Clients connected\n",
                        clientList.Count,
                        _maxClientCount);

                    returnString += "\nClient Name | Last Heard | Client Address/Port\n\n";

                    lock (clientList)
                    {
                        foreach (var client in clientList)
                        {
                            returnString += string.Format("{0}         {1}            {2}\n", client.Guid,
                                client.LastHeard,
                                client.RemoteEp);
                        }
                    }

                    returnString += string.Format("\n{0}/{1} Clients connected\n",
                        clientList.Count,
                        _maxClientCount);

                    break;
                }
                case "BROADCAST":
                {
                    bool stayInLoop = true;
                    while (stayInLoop)
                    {
                        Console.Write("\nEnter broadcast text : ");
                        string broadcastString = Console.ReadLine();

                        if (broadcastString == "exit")
                        {
                            stayInLoop = false;
                        }

                        ClientBroadcast(broadcastString);
                    }
                    break;
                }
                case "CLS":
                {
                    Console.Clear();
                    break;
                }
                default:
                {
                    returnString = string.Format("Command {0} is invalid.\n", command);
                    break;
                }
            }
            return returnString;
        }

        private void ClientTimerTick(object source, ElapsedEventArgs eea)
        {
            // This list will be used to hold references to the clients that can be disposed.
            var clientsToDispose = new List<Client>();

            // Lock the clientList to prevent it from being modified by another thread.
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

        public void ClientBroadcast(string broadcastString)
        {
            lock (clientList)
            {
                foreach (Client client in clientList)
                {
                    client.SendString(broadcastString);
                }
            }
        }

        public bool AddClient(Client client)
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

        public void RemoveClient(Client client)
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
            lock (requestBroker)
            {
                returnBool = requestBroker.AddRequest(guid);
            }
            return returnBool;
        }

        public bool RemoveClientRequest(string guid)
        {
            bool returnBool;
            lock (requestBroker)
            {
                returnBool = requestBroker.RemoveRequest(guid);
            }
            return returnBool;
        }

        private void CheckEnvironment()
        {
            _logicalCoreCount = Environment.ProcessorCount;
            _maxClientCount = _logicalCoreCount*5;
            _maxClientRequests = _logicalCoreCount*2;
        }
    }
}