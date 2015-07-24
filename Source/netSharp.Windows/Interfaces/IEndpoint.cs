using netSharp.Sessions;

namespace netSharp.Interfaces
{
    public interface IBaseEndpoint
    {    
        void RemoveSession(Session session);
        void AddSession(Session session);
    }
}