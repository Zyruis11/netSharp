namespace Server.Objects.Server.Request
{
    public class Request
    {
        public string RequestVar;
        public string ResponseVar;
        public bool WorkComplete;

        public Request(string requestString)
        {
            RequestVar = requestString;
        }
    }
}