using netSharp.Core.Data;

namespace netSharp.Core.Interfaces
{
    public interface ISession
    {
        void ReadDataAsync();
        void SendDataAsync(DataStream dataStream);
    }
}