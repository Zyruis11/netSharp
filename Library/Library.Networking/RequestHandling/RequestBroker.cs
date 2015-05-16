using System.Collections.Generic;
using Library.Networking.TCP;

namespace Library.Networking.RequestHandling
{
    public class RequestBroker
    {
        private readonly int _maxClientRequests;
        private readonly List<string> clientsActiveRequestList;
        private Server _server;

        public RequestBroker(Server server)
        {
            _server = server;
            clientsActiveRequestList = new List<string>();
        }

        public bool AddRequest(string clientGuid)
        {
            if (clientsActiveRequestList.Count < _server.MaxClientRequests)
            {
                clientsActiveRequestList.Add(clientGuid);
                return true;
            }
            return false;
        }

        public bool RemoveRequest(string clientGuid)
        {
            try
            {
                clientsActiveRequestList.Remove(clientGuid);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}