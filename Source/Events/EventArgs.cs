namespace netSharp.Events
{
    public class EventDataArgs
    {
        public EventDataArgs(string @message = null, byte[] @byte = null)
        {
            if (message != null)
            {
                Message = @message;
            }
            ;
            if (@byte != null)
            {
                ByteArray = @byte;
            }
        }

        public string Message { get; set; }
        public byte[] ByteArray { get; set; }
    }
}