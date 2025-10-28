// Program.cs
using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;
using System;
using System.Threading.Tasks;

namespace OpcUaServer
{
    class Program
    {
        private static MyNodeManager _myNodeManager;
        private static Random _random = new Random();
        private static Timer _timer;
        public static async Task Main(string[] args)
        {
           
            var application = new ApplicationInstance
            {
                ApplicationName = "My First OPC UA Server",
                ApplicationType = ApplicationType.Server,
                ConfigSectionName = "MyFirstServer"
            };

            // Configure and build the server
            await application.LoadApplicationConfigurationAsync(false);
            bool haveCert = await application.CheckApplicationInstanceCertificatesAsync(true, 2048);
            if (!haveCert)
            {
                throw new Exception("Could not create a valid certificate.");
            }
            await application.CheckApplicationInstanceCertificatesAsync(false, 0);
          

            // This is our custom OPC UA Server instance
            var server = new MyServer();
            


            // Start the server
            await application.StartAsync(server);

            Console.WriteLine("Server started. Press enter to exit.");
            var masterNodeManager = server.CurrentInstance.NodeManager as MasterNodeManager;
            if (masterNodeManager != null)
            {
                // Find your custom NodeManager inside it
                _myNodeManager = masterNodeManager.NodeManagers
                    .OfType<MyNodeManager>()
                    .FirstOrDefault();

                if (_myNodeManager != null)
                {
                    _myNodeManager.AddDynamicNode("Temperature", 25.5);
                    _myNodeManager.AddDynamicNode("Pressure", 25);
                    _myNodeManager.AddDynamicNode("watervalue", 9);
                    _timer = new Timer(UpdateNodes, null, 1000, 1000);
                }
                else
                {
                    Console.WriteLine("MyNodeManager not found!");
                }
                Console.ReadLine();

                // Stop the server
                application.Stop();
            }
        }

        private static void UpdateNodes(object? state)
        {
            if (_myNodeManager == null) return;

            // Generate random values or any logic
            double temp = Math.Round(20 + _random.NextDouble() * 10, 2);    // 20 - 30
            double pressure = Math.Round(10 + _random.NextDouble() * 5, 2); // 10 - 15
            double water = Math.Round(_random.NextDouble() * 100, 2);       // 0 - 100

            // Update dynamic nodes
            _myNodeManager.UpdateNodeValue("Temperature", temp);
            _myNodeManager.UpdateNodeValue("Pressure", pressure);
            _myNodeManager.UpdateNodeValue("watervalue", water);

            Console.WriteLine($"Updated nodes: Temperature={temp}, Pressure={pressure}, WaterValue={water}");
        }
    }
}
