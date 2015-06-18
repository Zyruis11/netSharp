using netSharp.Core.Data;

namespace netSharp.Windows.Interfaces
{
    public interface ISession
    {
        void ReadDataAsync();
        void SendDataAsync(DataStream dataStream);
    }
}