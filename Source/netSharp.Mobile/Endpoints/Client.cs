using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using netSharp.Core.Helpers;
using netSharp.Mobile.Connectivity;

namespace netSharp.Mobile.Endpoints
{
    public class Client
    {
        //private Timer _clientTimer;
        public string ClientGuid { get; set; }
        public List<MobileSession> SessionList { get; set; } 

        public Client()
        {
            ClientGuid = ShortGuid.NewShortGuid();
            SessionList = new List<MobileSession>();
        }

        // TODO: Add Connect and Send methods and Recieve Event
    }
}
