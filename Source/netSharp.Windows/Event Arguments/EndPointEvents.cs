using netSharp.Data;
using netSharp.Sessions;

namespace netSharp.Event_Arguments
{
    public class EndpointEvents
    {
        public EndpointEvents(DataStream dataStream = null, Session sessionArg = null, string MessageArg = null)
        {
            if (dataStream  != null)
            {
                DataStream = dataStream;
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