using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using netSharp.Configuration;
using netSharp.Endpoint;
using netSharp.Factories.Endpoint;

namespace APIUser
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.GetClientEndpoint();
        }

        private Client client;

        private void GetClientEndpoint()
        {
            var clientFactory = new ClientFactory();
            var clientConfig = new ClientConfiguration();
            clientConfig.MaxSessions = 10;
            client = clientFactory.MakeNew(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000), clientConfig);
        }

        private Server server;

        private void GetServerEndpoint()
        {
            var serverFactory = new ServerFactory();
            var serverConfig = new ServerConfiguration();
            bool configLoaded = serverConfig.LoadConfigurationFile("C:\\Configs\\Config.text");
            server = serverFactory.MakeNew(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000), serverConfig);
        }


    }
}
