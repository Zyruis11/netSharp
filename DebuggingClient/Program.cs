using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DebuggingClient
{
    internal class Program : IDisposable
    {
        private readonly ASCIIEncoding encoder = new ASCIIEncoding();
        private NetworkStream _clientStream;
        private TcpClient _tcpClient = new TcpClient();
        private bool isDisposed;

        public void Dispose()
        {
            isDisposed = true;
        }

        private static void Main(string[] args)
        {
            var p = new Program();
            // Declare an instance of the Program class so that we can acess its non-static methods.
            p.InputLoop();
        }

        private void InputLoop()
        {
            while (!isDisposed)
            {
                Console.Write("Enter a command :");
                var line = Console.ReadLine();

                line = line.ToUpper();

                switch (line)
                {
                    case "EXIT":
                        {
                            isDisposed = true;
                            break;
                        }
                    case "CLOSE":
                        {
                            ServerClose();
                            break;
                        }
                    case "CONNECT":
                        {
                            ServerConnect();
                            break;
                        }
                    case "":
                        {
                            Console.WriteLine("Invalid Command.");
                            break;
                        }
                    default:
                        {
                            Sender(line);
                            break;
                        }
                }
            }
            ServerClose();
        }

        private void ServerConnect()
        {
            var serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
            _tcpClient = new TcpClient();
            _tcpClient.Connect(serverEndPoint);
           

            if (_tcpClient.Connected)
            {
                Console.WriteLine("Connection Succesful!");
                var recieverThread = new Thread(RecieverLoop);
                recieverThread.Start();
            }
        }

        private void ServerClose()
        {
            _tcpClient.Close();

            if (!_tcpClient.Connected)
            {
                Console.WriteLine("Connection Closed");
            }
        }

        private void Sender(string sendString)
        {
            if (_tcpClient.Connected)
            {
                var buffer = encoder.GetBytes(sendString);
                _clientStream.Write(buffer, 0, buffer.Length);
            }
            else if (!_tcpClient.Connected)
            {
                Console.WriteLine("No Connection Available :(");
            }
        }

        private void RecieverLoop()
        {
            _clientStream = _tcpClient.GetStream();

            byte[] message;
            int bytesRead;

            while (!isDisposed)
            {
                message = new byte[4096];
                bytesRead = 0;

                try
                {                 
                    bytesRead = _clientStream.Read(message, 0, 4096);   
                }
                catch(Exception exception)
                {
                    Console.Write("\n\n{0}\n\n", exception.Message);
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                Console.WriteLine("Server Response:" + encoder.GetString(message, 0, bytesRead));
            }
            _clientStream.Close();
            Console.WriteLine("Connection Closed");
        }
    }
}