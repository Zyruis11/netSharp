using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
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
        public bool IsInitialized;
        public RequestBroker requestBroker;
        private ServerListener serverListener;
        private System.Timers.Timer clientTimer;

        public void Intialize(string serverBindAddr, int serverBindPort)
        {
            Console.Write("Checking environment...\n\n");

            CheckEnvironment();

            Console.Write("{0} logical processors available.\n{1} client(s) supported.\n{2} concurrent requests supported.\n\n",
                _logicalCoreCount, _maxClientCount, _maxClientRequests);
            Console.Write("Bind Address ({0})\n", serverBindAddr);
            Console.Write("Bind Port ({0})\n\n", serverBindPort);

            serverListener = new ServerListener(this, IPAddress.Parse(serverBindAddr), serverBindPort);
            serverListener.StartListener();
            IsInitialized = true;

            clientTimer = new System.Timers.Timer();
            clientTimer.Elapsed += new ElapsedEventHandler(ClientTimerTick);
            clientTimer.Interval = 1000;
            clientTimer.Enabled = true;

            requestBroker = new RequestBroker(_maxClientRequests);

            Console.Write("Started at {0} \nListening for clients...\n\n", DateTime.Now);
        }

        private void ClientTimerTick(object source, ElapsedEventArgs eea)
        {
            // This list will be used to hold references to the clients that can be disposed.
            List<Client> clientsToDispose = new List<Client>();

            // Lock the clientList to prevent it from being modified by another thread.
            lock (clientList)
            {
                foreach (Client client in clientList)
                {
                    client.LastHeard += 1;

                    if (client.LastHeard >= 30)
                    {
                        client.Alive = false;
                        clientsToDispose.Add(client);
                    }
                }

                foreach (Client client in clientsToDispose)
                {
                    client.Dispose();
                    clientList.Remove(client);
                    Console.WriteLine("\nClient {0} timed out and was disposed.\n", client.guid);
                }
            }
        }

        public string ConsoleCommandProcessor(string command)
        {
            var returnString = "";
            var commandToUpper = command.ToUpper();
            // Commands are case-insensitive, convert the command string to upper case for evaluation but also preserve the original case for return.

            switch (commandToUpper)
            {
                case "LISTEN":
                {
                    if (serverListener.AcceptsNewClients)
                    {
                        returnString = "Listener already started.\n\n";
                    }

                    if (!serverListener.AcceptsNewClients)
                    {
                        serverListener.StartListener();
                        returnString = "Listening for clients...\n\n";
                    }

                    break;
                }
                case "NO LISTEN":
                {
                    if (!serverListener.AcceptsNewClients)
                    {
                        returnString = "Listener not started.\n\n";
                    }

                    if (serverListener.AcceptsNewClients)
                    {
                        serverListener.StopListener();
                        returnString = "Listener stopped.\n\n";
                    }
                    break;
                }
                case "CLIENTS":
                {
                    //to-do: Show Information on Main
                    returnString = string.Format("\n{0}/{1} Clients connected\nServer Listening : {2}\n",
                        clientList.Count,
                        _maxClientCount, serverListener.AcceptsNewClients);

                    returnString += "\nClient Name | Client Alive/Last Heard | Client Address/Port\n\n";

                    lock (clientList)
                    {
                        foreach (var client in clientList)
                        {
                            returnString += string.Format("{0}          {1}/{2}                   {3}\n", client.guid,
                                client.Alive,
                                client.LastHeard,
                                client._tcpClient.Client.RemoteEndPoint);
                        }
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

        public bool AddClient(Client client)
        {
            var addSuccess = false;

            if (clientList.Count < _maxClientCount)
            {
                lock (clientList)
                {
                    clientList.Add(client);
                }
                addSuccess = true;
                Console.WriteLine("\nNew client connected.\n");
            }

            return addSuccess;
        }

        public bool RemoveClient(Client client)
        {
            var removeSuccess = false;

            lock (clientList)
            {
                clientList.Remove(client);
            }
            removeSuccess = true;

            return removeSuccess;
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

            double doubleLCC = Convert.ToDouble(_logicalCoreCount);

            doubleLCC = doubleLCC/2;

            _maxClientRequests = Convert.ToInt32(doubleLCC);
        }
    }
}