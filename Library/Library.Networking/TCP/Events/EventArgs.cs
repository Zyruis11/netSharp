namespace netSharp.TCP.Events
{
    public class TcpEventArgs
    {
        public TcpEventArgs(string s = null, byte[] b = null)
        {
            if (s != null)
            {
                Message = s;
            }
            ;
            if (b != null)
            {
                ByteArray = b;
            }
        }

        public string Message { get; set; }
        public byte[] ByteArray { get; set; }
    }
}