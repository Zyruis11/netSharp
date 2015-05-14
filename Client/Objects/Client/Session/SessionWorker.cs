using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Objects.Client.Session
{
    internal class SessionWorker : IDisposable
    {
        private readonly Session _session;
        private readonly Task _sessionListenerTask;
        private readonly TcpClient _tcpClient;
        private bool _isDisposed;
        private NetworkStream _sessionStream;
        private ASCIIEncoding asciiEncoding;

        public SessionWorker(Session session)
        {
            _session = session;
            _tcpClient = _session.TcpClient;
            _sessionListenerTask = new Task(RecieverLoop);
            _sessionListenerTask.Start();
        }

        public void Dispose()
        {
            _sessionStream.Close();
            _isDisposed = true;
        }

        private void RecieverLoop()
        {
            _sessionStream = _tcpClient.GetStream();

            byte[] message;
            int bytesRead;

            while (!_isDisposed)
            {
                message = new byte[4096];
                bytesRead = 0;

                try
                {
                    bytesRead = _sessionStream.Read(message, 0, 4096);
                }
                catch (Exception exception)
                {
                    Console.Write("\n\n{0}\n\n", exception.Message);
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }
                asciiEncoding = new ASCIIEncoding();
                var serverString = asciiEncoding.GetString(message, 0, bytesRead);
                SessionRequestHandler(serverString);
            }
            _sessionStream.Close();
            Console.WriteLine("Connection Closed by Server");
        }

        private void SessionRequestHandler(string sessionString)
        {
            if (sessionString == null)
            {
                return;
            }

            if (sessionString.StartsWith("KA"))
            {
                SessionKeepaliveMechanism(sessionString);
                return;
            }

            if (sessionString.StartsWith("BC"))
            {
                return;
            }

            Console.WriteLine(sessionString);
        }

        private void SessionKeepaliveMechanism(string sessionString)
        {
            if (sessionString.ToUpper() == "KA_SERVER_HELLO")
            {
                Sender("KA_SERVER_HELLO_ACK");
                return;
            }

            if (sessionString.ToUpper() == "KA_CLIENT_HELLO_ACK")
            {
                Sender("KA_SERVER_HELLO_ACK");
                SendGuid();
                _session.LastHeard = 0;
            }
        }

        public void SendKeepaliveHello()
        {
            Sender("KA_CLIENT_HELLO");
        }

        public void SendGuid()
        {
            var sendString = "GUID_" + _session.Guid;
            Sender(sendString);
        }

        public void Sender(string sendString)
        {
            if (_tcpClient.Connected)
            {
                asciiEncoding = new ASCIIEncoding();
                var buffer = asciiEncoding.GetBytes(sendString);
                _sessionStream.Write(buffer, 0, buffer.Length);
            }
            else if (!_tcpClient.Connected)
            {
                Console.WriteLine("No Connection Available :(");
            }
        }
    }
}