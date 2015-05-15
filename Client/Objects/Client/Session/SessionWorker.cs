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
                RequestHandler(serverString);
            }
            _sessionStream.Close();
            Console.WriteLine("Connection Closed by Server");
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
                        //Guid = innerString;
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
                _session.LastHeard = 0;
            }
        }

        public void SendKeepaliveHello()
        {
            Sender("KAL_CLIENTHELLO");
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