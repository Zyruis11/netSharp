using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpIpServer
{
    class ClientWorker : IDisposable
    {
        public bool IsDisposed;

        private Client _client;
        private TcpClient _tcpClient;
        private NetworkStream _clientStream;
        private Thread _clientListenerThread;
        private ASCIIEncoding encoder = new ASCIIEncoding();

        public ClientWorker(Client client)
        {
            _client = client;
            _tcpClient = _client._tcpClient;
            _clientListenerThread = new Thread(Start);
            _clientListenerThread.Start();
        }

        private void Start()
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
                catch (Exception)
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                string clientString = encoder.GetString(message, 0, bytesRead);
                ClientRequestHandler(clientString);
            }
            _clientStream.Close();
        }

        public void ClientRequestHandler(string clientString)
        {
            _client.LastHeard = 0;

            if (clientString.ToUpper() == "TEST")
            {
                Responder("ACK");
                return;
            }

            Request request = new Request(clientString);

            workSimulator();

            request.ResponseVar = "DONE";

            Responder(request.ResponseVar);
        }

        public void  workSimulator()
        {
            for (int i = 0; i < 100000000; i++)
            {
                int test = 120*120;
            }
        }

        public void Responder(string responseString)
        {
            byte[] buffer = encoder.GetBytes(responseString);
            _clientStream.Write(buffer, 0, buffer.Length);
        }

        public void Dispose()
        {
            _clientStream.Close();
            IsDisposed = true;
        }

    }
}
