// Copyright (c) 2015 Daniel Elps <daniel.j.elps@gmail.com>
// 
// All rights reserved.
// 
// Redistribution and use of APIUser in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Daniel Elps nor the names of its contributors may be 
//   used to endorse or promote products derived from this software without 
//   specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System.Net;
using netSharp.Configuration;
using netSharp.Endpoint;
using netSharp.Factories.Endpoint;

namespace APIUser
{
    internal class Program
    {
        private Client client;
        private Server server;

        private static void Main(string[] args)
        {
            var program = new Program();
            program.GetClientEndpoint();
        }

        private void GetClientEndpoint()
        {
            var clientFactory = new ClientFactory();
            var clientConfig = new ClientConfiguration();
            clientConfig.MaxSessions = 10;
            client = clientFactory.MakeNew(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000), clientConfig);
        }

        private void GetServerEndpoint()
        {
            var serverFactory = new ServerFactory();
            var serverConfig = new ServerConfiguration();
            var configLoaded = serverConfig.LoadConfigurationFile("C:\\Configs\\Config.text");
            server = serverFactory.MakeNew(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000), serverConfig);
        }
    }
}