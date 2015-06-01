using netSharp.Core.Data;
using netSharp.Mobile.Connectivity;

namespace netSharp.Mobile.Events
{
    public class MobileEvents
    {
        public MobileEvents(DataStream dataStreamArg = null, MobileSession sessionArg = null, string MessageArg = null)
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
        public MobileSession SessionReference { get; set; }
    }
}