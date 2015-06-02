using System.Diagnostics.Eventing.Reader;
using netSharp.Core.Data;
using netSharp.Server.Connectivity;

namespace netSharp.Core.Interfaces
{
    public interface IClient
    {
        void SendDataAsync(byte[] payload , Session session);
        void AddSession(Session session);
        void RemoveSession(Session session);
    }
}