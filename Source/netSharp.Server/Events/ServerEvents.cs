using netSharp.Core.Data;
using netSharp.Server.Connectivity;

namespace netSharp.Server.Events
{
    public class ServerEvents
    {
        public ServerEvents(DataStream dataStreamArg = null, Session sessionArg = null, string MessageArg = null)
        {
            if (dataStreamArg  != null)
            {
                DataStream = dataStreamArg;
            }
            if (sessionArg != null)
            {
                SessionReference = sessionArg;
            }
            if (MessageArg != null)
            {
                Message = MessageArg;
            }
        }

        public string Message { get; set; }
        public DataStream DataStream { get; set; }
        public Session SessionReference { get; set; }
    }
}