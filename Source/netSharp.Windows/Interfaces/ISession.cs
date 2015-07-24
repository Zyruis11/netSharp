using netSharp.Data;

namespace netSharp.Interfaces
{
    public interface ISession
    {
        void ReadDataAsync();
        void SendDataAsync(DataStream dataStream);
    }
}