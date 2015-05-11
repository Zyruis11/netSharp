using System.Collections.Generic;

namespace Server.Objects.Request
{
    // Request Broker provides a lockable object that all clients will access before submitting requests that 
    // may result in long runtimes. The maximum number of concurrent requests is calculated based on the number
    // of logical CPU's in the host system and passed to this object when it is instantiated via the default
    // constructor.

    public class RequestBroker
    {
        private readonly int _maxClientRequests;
        private readonly List<string> clientsActiveRequestList;

        public RequestBroker(int maxClientRequests)
        {
            _maxClientRequests = maxClientRequests;
            clientsActiveRequestList = new List<string>();
        }

        public bool AddRequest(string clientGuid)
        {
            if (clientsActiveRequestList.Count < _maxClientRequests)
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