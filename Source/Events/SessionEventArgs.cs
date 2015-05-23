using System.IO;
using netSharp.Objects;

namespace netSharp.Events
{
    public class SessionEventArgs
    {
        public SessionEventArgs(DataStream dataStreamArg = null, Session sessionArg = null)
        {
            if (dataStreamArg  != null)
            {
                DataStream = dataStreamArg;
            }
            if (sessionArg != null)
            {
                SessionReference = sessionArg;
            }
        }

        public string Message { get; set; }
        public byte[] ByteArray { get; set; }
        public DataStream DataStream { get; set; }
        public Session SessionReference { get; set; }
    }
}