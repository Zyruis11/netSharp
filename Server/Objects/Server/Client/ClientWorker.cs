using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server.Objects.Server.Client
{
    internal class ClientWorker : IDisposable
    {
        private readonly Client _client;
        private readonly Thread _clientListenerThread;
        private readonly TcpClient _tcpClient;
        private NetworkStream _clientStream;
        private ASCIIEncoding asciiEncoding;
        public bool IsDisposed;

        public ClientWorker(Client client)
        {
            _client = client;
            _tcpClient = _client.TcpClient;
            _clientListenerThread = new Thread(RecieverLoop);
            _clientListenerThread.Start();
        }

        public void Dispose()
        {
            _clientStream.Close();
            IsDisposed = true;
        }

        private void RecieverLoop()
        {
            _clientStream = _tcpClient.GetStream();

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
                    //Console.Write("\n\n{0}\n\n", exception.Message);
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }
                asciiEncoding = new ASCIIEncoding();
                var clientString = asciiEncoding.GetString(message, 0, bytesRead);
                ClientRequestHandler(clientString);
            }
            _clientStream.Close();
        }

        public void ClientRequestHandler(string clientString)
        {
            if (clientString == null)
            {
                return;
            }

            if (clientString.ToUpper() == "TEST")
            {
                Responder("ACK");
                return;
            }

            if (clientString.StartsWith("KA"))
            {
                SessionKeepaliveMechanism(clientString);
                return;
            }

            if (clientString.StartsWith("GUID_"))
            {
                _client.Guid = clientString.Remove(0, 5);
                return;
            }

            var request = new Request.Request(clientString);
            var requestAdded = _client.Server.AddClientRequest(_client.Guid);

            if (requestAdded)
            {
                WorkSimulator(request);
                _client.CurrentStatus = "Processing Request";
                _client.Server.RemoveClientRequest(_client.Guid);
                Responder(request.ResponseVar);
            }
            else if (!requestAdded)
            {
                Responder("ERR_SERVERBUSY");
            }
        }

        private void SessionKeepaliveMechanism(string clientString)
        {
            if (clientString.ToUpper() == "KA_CLIENT_HELLO")
            {
                Responder("KA_CLIENT_HELLO_ACK");
                return;
            }

            if (clientString.ToUpper() == "KA_SERVER_HELLO_ACK")
            {
                _client.LastHeard = 0;
            }
        }

        public void WorkSimulator(Request.Request request)
        {
            Thread.Sleep(5000);
            request.ResponseVar = "Work Completed.";
        }

        public void Responder(string responseString)
        {
            asciiEncoding = new ASCIIEncoding();
            var buffer = asciiEncoding.GetBytes(responseString);
            _clientStream.Write(buffer, 0, buffer.Length);
        }
    }
}