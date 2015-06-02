using System.Collections.Generic;
using netSharp.Server.Connectivity;

namespace netSharp.Server.Interfaces
{
    public interface IServer
    {
        void SendDataAsync(byte[] payload, Session session);
        void StartListener();
        void StopListener();
        void RemoveSession(Session session);
        void AddSession(Session session);
    }
}