using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server.Objects.Server.Client
{
    internal class Client : IDisposable
    {
        private Thread _clientListenerThread;
        private NetworkStream _clientStream;
        private ASCIIEncoding asciiEncoding;
        public Request.Request clientRequest;
        public string CurrentStatus;
        public string Guid;
        public bool IsDisposed;
        public int LastHeard;
        public string RemoteEp;
        public Server Server;
        public TcpClient TcpClient;

        public Client(Server server, TcpClient tcpClient)
        {
            Server = server;
            TcpClient = tcpClient;
            LastHeard = 0;
            RemoteEp = TcpClient.Client.RemoteEndPoint.ToString();
        }

        public void Dispose()
        {
            Close();
            IsDisposed = true;
        }

        public void Close()
        {
            TcpClient.Close();

            if (!TcpClient.Connected)
            {
                Console.WriteLine("Connection Closed");
            }
        }

        public void StartReciever()
        {
            _clientListenerThread = new Thread(Reciever);
            _clientListenerThread.Start();
        }

        public void SendString(string responseString)
        {
            asciiEncoding = new ASCIIEncoding();
            var buffer = asciiEncoding.GetBytes(responseString);
            _clientStream.Write(buffer, 0, buffer.Length);
        }

        private void Reciever()
        {
            Thread.Sleep(1000); // Slow start to ensure that if we load a new reciever during error recovery the errant reciever 
                                // has time to release resources.
            _clientStream = TcpClient.GetStream();

            byte[] message;
            int bytesRead;

            while (!IsDisposed)
            {
                message = new byte[4096];
                bytesRead = 0;

                try
                {
                    bytesRead = _clientStream.Read(message, 0, 4096);
                }
                catch (Exception exception)
                {
                    StartReciever(); // Start a new reciever
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }
                asciiEncoding = new ASCIIEncoding();
                var clientString = asciiEncoding.GetString(message, 0, bytesRead);
                RequestHandler(clientString);
            }
            if (!IsDisposed)
            {
                _clientStream.Close(); // Only close the client stream if we are disposing the object. Leave it open if we need to
                                       // break out of this reciever for error recovery so the next reciever can pick up the clientstream.
            }
        }

        public void RequestHandler(string clientString)
        {
            string[] clientStringArray = clientString.Split('_');

            string commandCharacters = clientStringArray[0];
            string innerString = clientStringArray[1];

            switch (commandCharacters)
            {
                case "CMD":
                {
                    break;
                }

                case "KAL":
                {
                    SessionKeepaliveMechanism(innerString);
                    break;
                }

                case "AID":
                {
                    Guid = innerString;
                    break;
                }
                default:
                {
                    SendString("Unexpected request type recieved.");
                    break;
                }
            }
        }

        private void SessionKeepaliveMechanism(string clientString)
        {
            if (clientString.ToUpper() == "CLIENTHELLO")
            {
                SendString("KAL_CLIENTHELLOACK");
                return;
            }

            if (clientString.ToUpper() == "SERVERHELLOACK")
            {
                LastHeard = 0;
            }
        }
    }
}