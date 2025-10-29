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
        private static CancellationTokenSource _cts = new CancellationTokenSource();
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
                    // STEP 1: Get node info from SQL (mocked here)
                    var nodesFromDb = await GetNodesFromDatabaseAsync();

                    // STEP 2: Create the nodes dynamically
                    foreach (var node in nodesFromDb)
                    {
                        NodeId dataType = node.DataType switch
                        {
                            "Double" => DataTypeIds.Double,
                            "Int32" => DataTypeIds.Int32,
                            "Int64" => DataTypeIds.Int64,
                            "String" => DataTypeIds.String,
                            _ => DataTypeIds.BaseDataType
                        };

                        _myNodeManager.AddDynamicNode(node.NodeName, node.InitialValue, dataType);
                    }

                    // STEP 3: Start independent update loops for each node
                    StartNodeUpdates(nodesFromDb);
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

        private static Task<List<NodeInfo>> GetNodesFromDatabaseAsync()
        {
            var list = new List<NodeInfo>
        {
            new NodeInfo { NodeName = "Temperature", DataType = "Double", InitialValue = 25.5, UpdateIntervalMs = 100 },
            new NodeInfo { NodeName = "Pressure", DataType = "Double", InitialValue = 10.2, UpdateIntervalMs = 100 },
            new NodeInfo { NodeName = "WaterLevel", DataType = "Int32", InitialValue = 80, UpdateIntervalMs = 100 },
            new NodeInfo { NodeName = "StringValue", DataType = "String", InitialValue = "OPC Server Running", UpdateIntervalMs = 100 }
        };
            return Task.FromResult(list);
        }
        // Random value generator (mock sensor data)
        private static object GenerateRandomValueForNode(NodeInfo node)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return node.DataType switch
            {
                "Double" => Math.Round(20 + _random.NextDouble() * 10, 2),
                "Int32" => _random.Next(0, 100),
                "Int64" => (long)_random.Next(1000, 10000),
                "String" => $"String_{_random.Next(1, 100)}",
                _ => null
            };
#pragma warning restore CS8603 // Possible null reference return.
        }
        // Start async task for each node
        private static void StartNodeUpdates(IEnumerable<NodeInfo> nodes)
        {
            foreach (var node in nodes)
            {

                _ = Task.Run(async () =>
                {
                    var nextTick = DateTime.UtcNow;
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        object newValue = GenerateRandomValueForNode(node);
                        await _myNodeManager.UpdateNodeValue(node.NodeName, newValue);
                        nextTick = nextTick.AddMilliseconds(1000);
                        var delay = nextTick - DateTime.UtcNow;
                        if (delay > TimeSpan.Zero)
                            await Task.Delay(delay);
                        else
                            nextTick = DateTime.UtcNow; // we fell behind, reset
                        //Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Updated {node.NodeName} = {newValue} (every {node.UpdateIntervalMs} ms)");

                        //await Task.Delay(node.UpdateIntervalMs, _cts.Token);
                    }
                }, _cts.Token);
            }
        }
        //private static void UpdateNodes(object? state)
        //{
        //    if (_myNodeManager == null) return;

        //    // Generate random values or any logic
        //    double temp = Math.Round(20 + _random.NextDouble() * 10, 2);    // 20 - 30
        //    double pressure = Math.Round(10 + _random.NextDouble() * 5, 2); // 10 - 15
        //    double water = Math.Round(_random.NextDouble() * 100, 2);       // 0 - 100

        //    // Update dynamic nodes
        //    _myNodeManager.UpdateNodeValue("Temperature", temp);
        //    _myNodeManager.UpdateNodeValue("Pressure", pressure);
        //    _myNodeManager.UpdateNodeValue("watervalue", water);

        //    Console.WriteLine($"Updated nodes: Temperature={temp}, Pressure={pressure}, WaterValue={water}");
        //}
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
    public class NodeInfo
    {
        public string NodeName { get; set; }
        public string DataType { get; set; }
        public object InitialValue { get; set; }
        public int UpdateIntervalMs { get; set; }
    }
}
