
// MyServer.cs
using Opc.Ua;
using Opc.Ua.Server;
using System;
using System.Collections.Generic;

namespace OpcUaServer
{
    public class MyServer : StandardServer
    {
        // Called when creating the MasterNodeManager
        protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        {
            // Add your custom NodeManager(s)
            var nodeManagers = new List<INodeManager>
            {
                new MyNodeManager(server, configuration)
            };

            return new MasterNodeManager(server, configuration, null, nodeManagers.ToArray());
        }

        // Called after server has started
        protected override void OnServerStarting(ApplicationConfiguration configuration)
        {
            base.OnServerStarting(configuration);

            Console.WriteLine("Server is starting...");
        }

        // Called after the server has fully started
        protected override void OnServerStarted(IServerInternal server)
        {
            base.OnServerStarted(server);

            Console.WriteLine("Server started successfully!");

            // Print all available endpoints
            Console.WriteLine("Available Endpoints:");
            var endpoints = this.GetEndpoints();

            foreach (var endpoint in endpoints)
            {
                Console.WriteLine($"URL: {endpoint.EndpointUrl}");
                //Console.WriteLine($"SecurityPolicy: {endpoint.SecurityPolicyUri}");
                //Console.WriteLine($"SecurityMode: {endpoint.SecurityMode}");
                //Console.WriteLine($"TransportProfile: {endpoint.TransportProfileUri}");
                Console.WriteLine("--------------------------------------------");
            }

            Console.WriteLine("Server is ready for LAN connections.");
        }
    }
}


