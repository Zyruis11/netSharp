using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Networking.TCP
{
    public class ClientSession : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private ASCIIEncoding _asciiEncoding;
        private NetworkStream _clientStream;
        private Task _streamReaderThread;

        /// <summary>
        /// Default constructor, accepts a fully initialized and connected TcpClient.
        /// </summary>
        /// <param name="tcpClient"></param>
        public ClientSession(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            RemoteClientEndpointIpAddressPort = _tcpClient.Client.RemoteEndPoint.ToString();
            IsDisposed = false;
            LastHeard = 0;
            StartStreamReader();
        }

        public string ClientGuid { get; set; }
        public bool IsDisposed { get; set; }
        public int LastHeard { get; set; }
        public string RemoteClientEndpointIpAddressPort { get; set; }

        public void Dispose()
        {
            _clientStream.Close();

            if (_tcpClient.Connected)
            {
                _tcpClient.Close();
            }

            IsDisposed = true;
        }

        public void SendString(string responseString)
        {
            _asciiEncoding = new ASCIIEncoding();
            var buffer = _asciiEncoding.GetBytes(responseString);
            _clientStream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Recieves KAL_ messages and sets keepalive variables based on those messages.
        /// </summary>
        /// <param name="clientString"></param>
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

        public void StartStreamReader()
        {
            _streamReaderThread = new Task(StreamReader);
            _streamReaderThread.Start();
        }

        /// <summary>
        ///     Blocks on the NetworkStream of the TcpClient, it recieves data sent across
        ///     the stream and sends it to a parsing function for further processing.
        /// </summary>
        private void StreamReader()
        {
            // Slow start in case we need to wait for another streamReader to release the stream.
            Thread.Sleep(500);

            _clientStream = _tcpClient.GetStream();

            while (!IsDisposed)
            {
                _clientStream.Flush();

                var message = new byte[8192];
                var bytesRead = 0;

                try
                {
                    bytesRead = _clientStream.Read(message, 0, 8192); //to-do: Allow variable buffer size

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
                    _clientStream.Close();
                    StartStreamReader(); // Start a new reciever
                    break;
                }
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
                    //to-do: Fire event on server
                    break;
                }

                case "KAL":
                {
                    SessionKeepaliveMechanism(innerString);
                    break;
                }

                case "AID":
                {
                    ClientGuid = innerString;
                    break;
                }
                default:
                {
                    SendString("Unexpected request type recieved.");
                    break;
                }
            }
        }
    }
}