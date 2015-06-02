using netSharp.Core.Data;

namespace netSharp.Core.Interfaces
{
    public interface IClient
    {
        void SendDataAsync(byte[] payload , string guid = null);
        void CreateSession();
        void RemoveSession(string guid);
    }
}