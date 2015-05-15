using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using Server.Objects;
using Timer = System.Timers.Timer;

namespace Server
{
    internal class Server : IDisposable
    {
        private readonly Timer _clientTimer;
        private readonly IPAddress _listenAddress;
        private readonly int _listenPort;
        private bool _acceptExternalClients;
        private Thread _clientFactoryThread;
        private int _logicalCoreCount;
        public int _maxClientCount;
        private TcpListener _tcpListener;
        public List<ClientObject> ClientObjectList = new List<ClientObject>();
        public bool IsDisposed;
        public int MaxClientRequests;
        public RequestBroker requestBroker;

        public Server(string serverBindAddr, int serverBindPort)
        {
            _listenAddress = IPAddress.Parse(serverBindAddr);
            _listenPort = serverBindPort;

            _tcpListener = new TcpListener(_listenAddress, _listenPort);

            _clientTimer = new Timer(1000);
            _clientTimer.Elapsed += ClientTimerTick;
            _clientTimer.Enabled = true;

            CheckEnvironment();

            StartClientFactory();

            // ReSharper disable once InconsistentlySynchronizedField
            requestBroker = new RequestBroker(MaxClientRequests);
        }

        public void Dispose()
        {
            StopClientFactory();
            IsDisposed = true;
        }

        private void ClientTimerTick(object source, ElapsedEventArgs eea)
        {
            var clientsToDispose = new List<ClientObject>();

            lock (ClientObjectList)
            {
                foreach (var client in ClientObjectList)
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
                    ClientObjectList.Remove(client);
                }
            }
        }

        // Since the server is blocking in AcceptTcpClient we will connect to ourselves internally to allow the Client Accept Loop to be escaped.
        public void InternalTcpConnection()
        {
            var internalEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
            var internalTcpClient = new TcpClient();
            internalTcpClient.Connect(internalEndpoint);
            internalTcpClient.Close();
        }

        private void ClientFactory()
        {
            Thread.Sleep(1000);
         
            _tcpListener.Start();

            while (!IsDisposed)
            {
                try
                {
                    var tcpClient = _tcpListener.AcceptTcpClient();
                    if (_acceptExternalClients)
                    {
                        var clientObject = new ClientObject(tcpClient);

                        var addSuccess = AddClient(clientObject);

                        if (!addSuccess)
                            // If we cannot add the client, dispose the object. The dispose method on the client will automatically close the conenction.
                        {
                            clientObject.Dispose();
                        }
                        else // If we can add the client, start the worker thread.
                        {
                            clientObject.StartReciever();
                        }
                    }
                    else
                    {
                        tcpClient.Close();
                    }
                }
                catch (Exception)
                {
                    StartClientFactory(); // Start a new client factory on error
                    break;
                }
            }
            _tcpListener.Stop();
        }

        public void ClientBroadcast(string broadcastString)
        {
            lock (ClientObjectList)
            {
                foreach (var client in ClientObjectList)
                {
                    client.SendString(broadcastString);
                }
            }
        }

        public void StartClientFactory()
        {
            _acceptExternalClients = true;
            _clientFactoryThread = new Thread(ClientFactory);
            _clientFactoryThread.Start();
        }

        public void StopClientFactory()
        {
            _acceptExternalClients = false;
            InternalTcpConnection();
        }

        public bool AddClient(ClientObject client)
        {
            lock (ClientObjectList)
            {
                if (ClientObjectList.Count < _maxClientCount)
                {
                    ClientObjectList.Add(client);

                    return true;
                }
            }
            return false;
        }

        public void RemoveClient(ClientObject client)
        {
            lock (ClientObjectList)
            {
                if (ClientObjectList.Contains(client))
                {
                    ClientObjectList.Remove(client);
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
            MaxClientRequests = _logicalCoreCount*2;
        }
    }
}