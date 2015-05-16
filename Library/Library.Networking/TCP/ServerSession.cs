using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Networking.TCP
{
    public class ServerSession : IDisposable
    {
        private TcpClient _tcpClient;
        private ASCIIEncoding _asciiEncoding;
        private NetworkStream _sessionStream;
        private Task _streamReaderTask;

        public ServerSession(string sessionEndpoint, string clientServerGuid)
        {
            var sessionEndpointParse = sessionEndpoint.Split(':');
            var endpointIpAddress = sessionEndpointParse[0];
            var endpointPort = Convert.ToInt32(sessionEndpointParse[1]);
            var serverEndPoint = new IPEndPoint(IPAddress.Parse(endpointIpAddress), endpointPort);
            ServerGuid = clientServerGuid;
            LastHeard = 0;
            HelloInterval = 1; // First Hello interval is short to speed GUID exchange. After first hello, interval length will be set by to the length defined on the server.
            Connect(serverEndPoint);
        }

        public string ServerGuid { get; set; } // For future use
        public bool IsDisposed { get; set; }
        public int LastHeard { get; set; }
        public string RemoteServerEndpointIpAddressPort { get; set; }
        public int HelloInterval { get; set; }

        public void Dispose()
        {
            Close();
            IsDisposed = true;
        }

        private void Connect(IPEndPoint serverEndPoint)
        {
            _tcpClient = new TcpClient();

            _tcpClient.Connect(serverEndPoint);

            if (_tcpClient.Connected)
            {
                _sessionStream = _tcpClient.GetStream();
                StartStreamReader();
            }
        }

        public void Close()
        {
            _tcpClient.Close();

            if (!_tcpClient.Connected)
            {
                Console.WriteLine("Connection Closed");
            }
        }

        private void StartStreamReader()
        {
            _streamReaderTask = new Task(StreamReader);
            _streamReaderTask.Start();
        }

        private void StreamReader()
        {
            Thread.Sleep(500);

            while (!IsDisposed)
            {
                _sessionStream.Flush();

                var message = new byte[8192];
                var bytesRead = 0;

                try
                {
                    bytesRead = _sessionStream.Read(message, 0, 8192); //to-do: Allow variable buffer size

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
                    _sessionStream.Close();
                    StartStreamReader();
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
                    //ServerGuid = innerString;
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        private void SessionKeepaliveMechanism(string sessionString)
        {
            if (sessionString.ToUpper() == "SERVERHELLO")
            {
                Sender("KAL_SERVERHELLOACK");
                return;
            }

            if (sessionString.ToUpper() == "CLIENTHELLOACK")
            {
                Sender("KAL_SERVERHELLOACK");
                SendGuid();
                LastHeard = 0;
            }
        }

        public void SendKeepaliveHello()
        {
            Sender("KAL_CLIENTHELLO");
        }

        public void SendGuid()
        {
            var sendString = "AID_" + ServerGuid;
            Sender(sendString);
        }

        public void Sender(string sendString)
        {
            if (_tcpClient.Connected)
            {
                _asciiEncoding = new ASCIIEncoding();
                var buffer = _asciiEncoding.GetBytes(sendString);
                _sessionStream.Write(buffer, 0, buffer.Length);
            }
            else if (!_tcpClient.Connected)
            {
                throw new Exception("Connection not available.");
            }
        }
    }
}