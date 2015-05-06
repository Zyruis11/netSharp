namespace TcpIpServer.Classes
{
    public class Request
    {
        public bool WorkComplete;
        public string RequestVar;
        public string ResponseVar;

        public Request(string requestString)
        {
            RequestVar = requestString;
        }
    }
}
