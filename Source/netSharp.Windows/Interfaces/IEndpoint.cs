using netSharp.Windows.Connectivity;

namespace netSharp.Windows.Interfaces
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