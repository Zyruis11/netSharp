using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DebuggingClient.Classes
{
    internal class SessionWorker
    {
        private readonly Session _session;
        private readonly Task _sessionListenerTask;
        private readonly TcpClient _tcpClient;
        private readonly ASCIIEncoding encoder = new ASCIIEncoding();
        private NetworkStream _sessionStream;
        public bool IsDisposed;

        public SessionWorker(Session session)
        {
            _session = session;
            _tcpClient = _session.TcpClient;
            _sessionListenerTask = new Task(RecieverLoop);
            _sessionListenerTask.Start();
        }

        private void RecieverLoop()
        {
            _sessionStream = _tcpClient.GetStream();

            byte[] message;
            int bytesRead;

            while (!IsDisposed)
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

                var serverString = encoder.GetString(message, 0, bytesRead);
                SessionRequestHandler(serverString);
                Console.WriteLine(serverString);
            }
            _sessionStream.Close();
            Console.WriteLine("Connection Closed by Server");
        }

        private void SessionRequestHandler(string sessionString)
        {
            if (sessionString.ToUpper() == "SERVER_HELLO")
            {
                Sender("SERVER_HELLO_ACK");
                return;
            }

            if (sessionString.ToUpper() == "CLIENT_HELLO_ACK")
            {
                Sender("SERVER_HELLO_ACK");
                _session.LastHeard = 0;
            }
        }

        public void Sender(string sendString)
        {
            if (_tcpClient.Connected)
            {
                var buffer = encoder.GetBytes(sendString);
                _sessionStream.Write(buffer, 0, buffer.Length);
            }
            else if (!_tcpClient.Connected)
            {
                Console.WriteLine("No Connection Available :(");
            }
        }

        public void Dispose()
        {
            _sessionStream.Close();
            IsDisposed = true;
        }
    }
}