netSharp Documentation

Summary

netSharp is intended to be a highly scalable network communications framework with an easy to use API that enables users to easily add scalable, intelligent networking to any .NET 4.6 project.

Architecture

netSharp uses non-blocking I/O along with an event-driven architecture to make high scalability an intrinsic attribute of the framework. netSharp takes some inspiration from Node.js in that it is designed for a similar purpose, however it is designed with client-server and peer to peer TCP in mind and I’m also using it as a learning experience.

Object Model

netSharp creates Endpoints, Endpoints connect to each other via Sessions and send binary data with a netSharp header. Binary payloads are delivered directly to and from the client application by calling a public Endpoint method and referencing a Session. Session references are stored in a dictionary key value store for fast lookups.

Endpoints use a Session Manager that performs tasks such as session timeout, session keepalives, session disposal and load balancing. Future extensions will allow the Session Manager to communicate to Session Managers on other Server endpoints so that Endpoint-based Intelligent Cluster Load Balancing (EbICLB) can be achieved.

All Endpoints and Sessions are created using a Factory pattern, when the factory pattern is called, a configuration parameter can be passed to the new object if other than default settings are desired.

Session Timeout Scaling

In the current implementation, an endpoint can take advantage of the Session Timeout Scaling algorithm, this algorithm uses an adaptive max idle timer to manage server load. As the session count increases, max idle time is decreased for all sessions.

timeout = timeout - ((currentSessionCount/MaxSessionCount) x (timeout))

The algorithm takes the current session count divided by the max session count and multiples it by the max idle time to get a utilization percentage. The utilization percentage is then taken from the static maxIdleTime and subtracted from it to produce the new max idle time. A minimum max idle time is also implemented to prevent 
API Usage

To instantiate a new endpoint, call its respective factory. The example below returns a Client endpoint and subscribes an event handler to its SessionDataReceivedEvent. This event is fired whenever data is received on a Session that is created from this Endpoint to another Endpoint.

        private void Initiate()
        {
            var clientFactory = new ClientFactory();
            client = clientFactory.MakeNew(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000));
            client.SessionDataRecieved += SessionDataRecievedHandler;
        }
