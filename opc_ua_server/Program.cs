// Program.cs
using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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
            // Get LAN IP
            string lanIp = GetLocalIPv4();

            // Set BaseAddresses dynamically
            application.ApplicationConfiguration.ServerConfiguration.BaseAddresses.Clear();
            application.ApplicationConfiguration.ServerConfiguration.BaseAddresses.Add($"opc.tcp://{lanIp}:4840");

            // Optional: print endpoint
            Console.WriteLine("Server will run on: " + application.ApplicationConfiguration.ServerConfiguration.BaseAddresses[0]);
            DeleteOpcUaCertificates(); // remove old invalid certs
            bool haveCert = await application.CheckApplicationInstanceCertificatesAsync(false, 2048);
            if (!haveCert)
            {
                throw new Exception("Could not create a valid certificate.");
            }
            await application.CheckApplicationInstanceCertificatesAsync(false, 0);
          

            // This is our custom OPC UA Server instance
            var server = new MyServer();
            


            // Start the server
            await application.StartAsync(server);            
            var masterNodeManager = server.CurrentInstance.NodeManager as MasterNodeManager;
            if (masterNodeManager != null)
            {
                // Find your custom NodeManager inside it
                _myNodeManager = masterNodeManager.NodeManagers
                    .OfType<MyNodeManager>()
                    .FirstOrDefault();

                if (_myNodeManager != null)
                {
                    _myNodeManager.AddDynamicNode("Temperature", 25.5, DataTypeIds.Double);
                    _myNodeManager.AddDynamicNode("Pressure", 25,DataTypeIds.Int32);
                    _myNodeManager.AddDynamicNode("watervalue", 9, DataTypeIds.Int64);
                    _myNodeManager.AddDynamicNode("StringValue", "DACPL OPC Server", DataTypeIds.String);
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
        public static string GetLocalIPv4()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                // Skip loopback, tunnel, and virtual adapters
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                    continue;

                var ipProps = ni.GetIPProperties();
                foreach (var ip in ipProps.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        // Ignore APIPA addresses (169.x.x.x)
                        if (!ip.Address.ToString().StartsWith("169."))
                            return ip.Address.ToString();
                    }
                }
            }

            return "127.0.0.1"; // fallback
        }
        public static void DeleteOpcUaCertificates()
        {
            // Get the local application data folder
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Build the OPC UA PKI "own" folder path
            string pkiOwnFolder = Path.Combine(localAppData, "OPC Foundation", "pki", "own");

            if (Directory.Exists(pkiOwnFolder))
            {
                Console.WriteLine("Deleting old OPC UA certificates...");
                try
                {
                    DirectoryInfo di = new DirectoryInfo(pkiOwnFolder);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                    Console.WriteLine("Old certificates deleted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error deleting certificates: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("No existing certificates found.");
            }
        }
    }
}
