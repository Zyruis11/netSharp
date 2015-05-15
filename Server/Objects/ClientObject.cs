using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server.Objects
{
    internal class ClientObject : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private ASCIIEncoding _asciiEncoding;
        private NetworkStream _clientStream;
        private Thread _recieverThread;
        private Request ClientRequest;
        public string Guid;
        public bool IsDisposed;
        public int LastHeard;
        public string RemoteEndpoint;

        public ClientObject(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            LastHeard = 0;
            RemoteEndpoint = _tcpClient.Client.RemoteEndPoint.ToString();
        }

        public void Dispose()
        {
            Close();
            IsDisposed = true;
        }

        public void Close()
        {
            _clientStream.Close();

            if (_tcpClient.Connected)
            {
                _tcpClient.Close();
            }
        }

        public void StartReciever()
        {
            _recieverThread = new Thread(Reciever);
            _recieverThread.Start();
        }

        public void SendString(string responseString)
        {
            _asciiEncoding = new ASCIIEncoding();
            var buffer = _asciiEncoding.GetBytes(responseString);
            _clientStream.Write(buffer, 0, buffer.Length);
        }

        private void Reciever()
        {
            Thread.Sleep(1000);
            // Slow start to ensure that if we load a new reciever during error recovery the errant reciever 
            // has time to release resources.
            _clientStream = _tcpClient.GetStream();

            while (!IsDisposed)
            {
                _clientStream.Flush();

                var message = new byte[4096];
                var bytesRead = 0;

                try
                {
                    bytesRead = _clientStream.Read(message, 0, 4096);

                    if (bytesRead == 0)
                    {
                        break;
                    }
                    _asciiEncoding = new ASCIIEncoding();
                    var clientString = _asciiEncoding.GetString(message, 0, bytesRead);
                    RequestHandler(clientString);
                }
                catch (Exception)
                {
                    StartReciever(); // Start a new reciever
                    break;
                }
            }

            if (IsDisposed)
            {
                _clientStream.Close();
                // Only close the client stream if we are disposing the object. Leave it open if we need to
                // break out of this reciever for error recovery so the next reciever can pick up the clientstream.
            }
        }

        public void RequestHandler(string clientString)
        {
            var clientStringArray = clientString.Split('_');

            var commandCharacters = clientStringArray[0];
            var innerString = clientStringArray[1];

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