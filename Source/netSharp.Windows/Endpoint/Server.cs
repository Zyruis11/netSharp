using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using netSharp.Sessions;

namespace netSharp.Endpoint
{
    public class Server : BaseEndpoint, IDisposable
    {
        private TcpListener tcpListener { get; set; }

        public void StartListener()
        {
            ClientListener();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private async void ClientListener()
        {
            tcpListener.Start();

            while (!IsDisposed)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
                if (!IsDisposed)
                {
                    if (SessionDictionary.Count >= MaxSessionCount) throw new Exception("Unable to add session");
                    var clientObject = new Session(tcpClient);
                    AddSession(clientObject);
                }
            }

            tcpListener.Stop();
        }
    }
}
