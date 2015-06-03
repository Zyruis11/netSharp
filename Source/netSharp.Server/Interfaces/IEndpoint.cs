using System.Collections.Generic;
using netSharp.Core.Data;
using netSharp.Server.Connectivity;

namespace netSharp.Server.Interfaces
{
    public interface IEndpoint
    {
        void SendDataAsync(byte[] payloadByteArray, Session session);
        void ReadDataAsync(Session session);
        void StartListener();
        //void StopListener();
        void RemoveSession(Session session);
        void AddSession(Session session);
    }
}