using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpIpServer.Classes
{
    internal class ClientWorker : IDisposable
    {
        private readonly Client _client;
        private readonly Thread _clientListenerThread;
        private readonly TcpClient _tcpClient;
        private readonly ASCIIEncoding encoder = new ASCIIEncoding();
        private NetworkStream _clientStream;
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

                var clientString = encoder.GetString(message, 0, bytesRead);
                ClientRequestHandler(clientString);
            }
            _clientStream.Close();
        }

        public void ClientRequestHandler(string clientString)
        {
            

            if (clientString.ToUpper() == "TEST")
            {
                Responder("ACK");
                return;
            }

            if (clientString.ToUpper() == "CLIENT_HELLO")
            {
                Responder("CLIENT_HELLO_ACK");
                return;
            }

            if (clientString.ToUpper() == "SERVER_HELLO_ACK")
            {
                _client.LastHeard = 0;
                return;
            }

            var request = new Request(clientString);

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

        public void WorkSimulator(Request request)
        {
            Thread.Sleep(5000);
            request.ResponseVar = "Work Completed.";
        }

        public void Responder(string responseString)
        {
            var buffer = encoder.GetBytes(responseString);
            _clientStream.Write(buffer, 0, buffer.Length);
        }
    }
}