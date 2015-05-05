using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpIpServer
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
