﻿namespace Server.Objects.Request
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