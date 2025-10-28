//// MyNodeManager.cs
//using Opc.Ua;
//using Opc.Ua.Server;
//using System;
//using System.Collections.Generic;
//using System.Threading;

//namespace OpcUaServer
//{
//    public class MyNodeManager : CustomNodeManager2
//    {
//        private const string MyNamespace = "http://MyFirstServer/custom/";
//        private Timer _timer;
//        private uint _counter;

//        public MyNodeManager(IServerInternal server, ApplicationConfiguration configuration)
//            : base(server, configuration, MyNamespace)
//        {
//            SystemContext.NodeIdFactory = this;
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                _timer?.Dispose();
//            }
//            base.Dispose(disposing);
//        }

//        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
//        {
//            CreateMyAddressSpace();
//        }

//        private void CreateMyAddressSpace()
//        {
//            var root = CreateFolder(null, "MyCompany");
//            var deviceFolder = CreateFolder(root, "Device");

//            // Add the counter variable node
//            var counterNode = CreateVariable(deviceFolder, "Counter", "Counter", BuiltInType.UInt32, ValueRanks.Scalar);
//            counterNode.AccessLevel = AccessLevels.CurrentRead;
//            counterNode.ValueRank = ValueRanks.Scalar;
//            counterNode.StatusCode = StatusCodes.Good;
//            counterNode.OnReadValue = OnReadCounterValue;


//            // Start a timer to update the counter value
//            _timer = new Timer(OnUpdate, null, 1000, 1000);
//        }
//        private ServiceResult OnReadCounterValue(
//     ISystemContext context,
//        NodeState node,
//        NumericRange indexRange,
//        QualifiedName dataEncoding,
//        ref object value,
//        ref StatusCode statusCode,
//               ref DateTime timestamp)
//        {
//            value = _counter;
//            statusCode = StatusCodes.Good;
//            timestamp = DateTime.UtcNow;
//            return ServiceResult.Good;
//        }


//        private void OnUpdate(object state)
//        {
//            _counter++;
//        }

//        private BaseDataVariableState CreateVariable(NodeState parent, string path, string name, BuiltInType type, int valueRank)
//        {
//            var node = new BaseDataVariableState(parent)
//            {
//                SymbolicName = name,
//                ReferenceTypeId = ReferenceTypes.Organizes,
//                TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
//                NodeId = new NodeId(path, NamespaceIndex),
//                BrowseName = new QualifiedName(path, NamespaceIndex),
//                DisplayName = new LocalizedText("en-US", name),
//                WriteMask = AttributeWriteMask.None,
//                UserWriteMask = AttributeWriteMask.None,
//                DataType = (uint)type,
//                ValueRank = valueRank,
//            };

//            parent?.AddChild(node);
//            return node;
//        }

//        private FolderState CreateFolder(NodeState parent, string path)
//        {
//            var folder = new FolderState(parent)
//            {
//                SymbolicName = path,
//                ReferenceTypeId = ReferenceTypes.Organizes,
//                TypeDefinitionId = ObjectTypeIds.FolderType,
//                NodeId = new NodeId(path, NamespaceIndex),
//                BrowseName = new QualifiedName(path, NamespaceIndex),
//                DisplayName = new LocalizedText("en-US", path),
//                WriteMask = AttributeWriteMask.None,
//                UserWriteMask = AttributeWriteMask.None,
//            };

//            parent?.AddChild(folder);
//            return folder;
//        }
//    }
//}

#region 2nd not working

//using Opc.Ua;
//using Opc.Ua.Server;
//using System;
//using System.Collections.Generic;
//using System.Threading;

//namespace OpcUaServer
//{
//    public class MyNodeManager : CustomNodeManager2
//    {
//        private const string MyNamespace = "http://MyFirstServer/custom/";
//        private FolderState _deviceFolder;
//        private Timer _timer;
//        private uint _counter;

//        public MyNodeManager(IServerInternal server, ApplicationConfiguration configuration)
//            : base(server, configuration, MyNamespace)
//        {
//            SystemContext.NodeIdFactory = this;
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                _timer?.Dispose();
//            }
//            base.Dispose(disposing);
//        }

//        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
//        {
//            CreateMyAddressSpace();
//        }

//        private void CreateMyAddressSpace()
//        {
//            // Root folder: MyCompany
//            var root = CreateFolder(null, "MyCompany");
//            AddPredefinedNode(SystemContext, root);

//            // Device folder
//            _deviceFolder = CreateFolder(root, "Device");
//            AddPredefinedNode(SystemContext, _deviceFolder);

//            // Counter node
//            var counterNode = CreateVariable(_deviceFolder, "Counter", BuiltInType.UInt32, 0u);
//            counterNode.OnReadValue = OnReadCounterValue;
//            AddPredefinedNode(SystemContext, counterNode);

//            // Start updating the counter every second
//            _timer = new Timer(OnUpdate, counterNode, 1000, 1000);
//        }

//        private ServiceResult OnReadCounterValue(
//            ISystemContext context,
//            NodeState node,
//            NumericRange indexRange,
//            QualifiedName dataEncoding,
//            ref object value,
//            ref StatusCode statusCode,
//            ref DateTime timestamp)
//        {
//            value = _counter;
//            statusCode = StatusCodes.Good;
//            timestamp = DateTime.UtcNow;
//            return ServiceResult.Good;
//        }

//        private void OnUpdate(object state)
//        {
//            _counter++;
//            var node = state as BaseDataVariableState;
//            if (node != null)
//            {
//                node.Value = _counter;
//                node.Timestamp = DateTime.UtcNow;
//                node.ClearChangeMasks(SystemContext, false);
//            }
//        }

//        private FolderState CreateFolder(NodeState parent, string name)
//        {
//            var folder = new FolderState(parent)
//            {
//                SymbolicName = name,
//                ReferenceTypeId = ReferenceTypes.Organizes,
//                TypeDefinitionId = ObjectTypeIds.FolderType,
//                NodeId = new NodeId(name, NamespaceIndex),
//                BrowseName = new QualifiedName(name, NamespaceIndex),
//                DisplayName = new LocalizedText("en-US", name),
//            };

//            parent?.AddChild(folder);
//            return folder;
//        }
//        private BaseDataVariableState CreateVariable(NodeState parent, string name, BuiltInType type, object initialValue)
//        {
//            var nsIndex = NamespaceIndex; // Correct namespace
//            var variable = new BaseDataVariableState(parent)
//            {
//                SymbolicName = name,
//                TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
//                NodeId = new NodeId(name, nsIndex), // readable NodeId
//                BrowseName = new QualifiedName(name, nsIndex),
//                DisplayName = new LocalizedText("en-US", name),
//                DataType = (uint)type,
//                ValueRank = ValueRanks.Scalar,
//                AccessLevel = AccessLevels.CurrentReadOrWrite,
//                UserAccessLevel = AccessLevels.CurrentReadOrWrite,
//                Value = initialValue,
//                StatusCode = StatusCodes.Good,
//                Timestamp = DateTime.UtcNow
//            };

//            parent?.AddChild(variable);
//            AddPredefinedNode(SystemContext, variable);
//            return variable;
//        }
//        // 🟩 Updated AddDynamicNode method
//        public BaseDataVariableState AddDynamicNode(string name, double initialValue)
//        {
//            if (_deviceFolder == null)
//            {
//                Console.WriteLine("[!] Device folder not initialized.");
//                return null;
//            }

//            var newNode = new BaseDataVariableState(_deviceFolder)
//            {
//                SymbolicName = name,
//                TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
//                NodeId = new NodeId(name, NamespaceIndex), // readable NodeId
//                BrowseName = new QualifiedName(name, NamespaceIndex),
//                DisplayName = new LocalizedText("en-US", name),
//                DataType = (uint)BuiltInType.Double,
//                ValueRank = ValueRanks.Scalar,
//                AccessLevel = AccessLevels.CurrentReadOrWrite,
//                UserAccessLevel = AccessLevels.CurrentReadOrWrite,
//                Value = initialValue,
//                StatusCode = StatusCodes.Good,
//                Timestamp = DateTime.UtcNow
//            };

//            _deviceFolder.AddChild(newNode);            // Add to folder
//            AddPredefinedNode(SystemContext, newNode);  // Register with server

//            // 🔹 Notify clients that folder changed
//            _deviceFolder.ClearChangeMasks(SystemContext, true);

//            Console.WriteLine($"[+] Added dynamic node '{name}' with initial value {initialValue}");
//            return newNode;
//        }



//        //private BaseDataVariableState CreateVariable(NodeState parent, string name, BuiltInType type, object initialValue)
//        //{
//        //    var variable = new BaseDataVariableState(parent)
//        //    {
//        //        SymbolicName = name,
//        //        ReferenceTypeId = ReferenceTypes.Organizes,
//        //        TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
//        //        NodeId = new NodeId(name, NamespaceIndex),
//        //        BrowseName = new QualifiedName(name, NamespaceIndex),
//        //        DisplayName = new LocalizedText("en-US", name),
//        //        DataType = (uint)type,
//        //        ValueRank = ValueRanks.Scalar,
//        //        AccessLevel = AccessLevels.CurrentReadOrWrite,
//        //        UserAccessLevel = AccessLevels.CurrentReadOrWrite,
//        //        Value = initialValue,
//        //        StatusCode = StatusCodes.Good,
//        //        Timestamp = DateTime.UtcNow
//        //    };

//        //    parent?.AddChild(variable);
//        //    return variable;
//        //}

//        // 🟩 Public method to add nodes dynamically
//        //public void AddDynamicNode(string name, double initialValue)
//        //{
//        //    var newNode = CreateVariable(_deviceFolder, name, BuiltInType.Double, initialValue);
//        //    AddPredefinedNode(SystemContext, newNode);
//        //    Console.WriteLine($"[+] Added dynamic node '{name}' with initial value {initialValue}");
//        //}

//        // 🟦 Method to update a node value dynamically
//        public void UpdateNodeValue(string nodeName, double newValue)
//        {
//            var children = new List<BaseInstanceState>();
//            _deviceFolder.GetChildren(SystemContext, children);

//            foreach (var child in children)
//            {
//                if (child.DisplayName.Text == nodeName && child is BaseDataVariableState variable)
//                {
//                    variable.Value = newValue;
//                    variable.Timestamp = DateTime.UtcNow;
//                    variable.ClearChangeMasks(SystemContext, false);
//                    Console.WriteLine($"[*] Updated node '{nodeName}' to value {newValue}");
//                    return;
//                }
//            }

//            Console.WriteLine($"[!] Node '{nodeName}' not found.");
//        }
//    }
//} 
#endregion
using Opc.Ua;
using Opc.Ua.Server;
using System;
using System.Collections.Generic;
using System.Threading;
using Opc.Ua;
using Opc.Ua.Server;

public class MyNodeManager : CustomNodeManager2
{
    private const string MyNamespaceUri = "http://MyFirstServer/custom/";
    private FolderState _deviceFolder;
    private Timer _timer;
    private uint _counter;

    public MyNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        : base(server, configuration, new string[] { MyNamespaceUri })
    {
        SystemContext.NodeIdFactory = this;
    }

    public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
    {
        // Create root folder (Device)
        _deviceFolder = new FolderState(null)
        {
            SymbolicName = "Device",
            NodeId = new NodeId("Device", NamespaceIndex),
            BrowseName = new QualifiedName("Device", NamespaceIndex),
            DisplayName = new LocalizedText("en-US", "Device"),
            TypeDefinitionId = ObjectTypeIds.FolderType
        };

        // Attach root folder to standard Objects folder
        if (!externalReferences.TryGetValue(ObjectIds.ObjectsFolder, out var references))
        {
            references = new List<IReference>();
            externalReferences[ObjectIds.ObjectsFolder] = references;
        }

        // Correct way to add reference
        references.Add(new NodeStateReference(
            ReferenceTypeIds.Organizes, // Reference type
            false,                      // Not inverse
            _deviceFolder.NodeId         // Target node
        ));


        AddPredefinedNode(SystemContext, _deviceFolder);

        // Add a counter variable
        var counterNode = new BaseDataVariableState(_deviceFolder)
        {
            SymbolicName = "Counter",
            NodeId = new NodeId("Counter", NamespaceIndex),
            BrowseName = new QualifiedName("Counter", NamespaceIndex),
            DisplayName = new LocalizedText("en-US", "Counter"),
            TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
            DataType = DataTypeIds.UInt32,
            ValueRank = ValueRanks.Scalar,
            AccessLevel = AccessLevels.CurrentReadOrWrite,
            UserAccessLevel = AccessLevels.CurrentReadOrWrite,
            Value = _counter
        };
        counterNode.OnReadValue = OnReadCounter;
        _deviceFolder.AddChild(counterNode);
        AddPredefinedNode(SystemContext, counterNode);

        // Timer to update counter every second
        _timer = new Timer(OnUpdateCounter, counterNode, 1000, 1000);
    }

    private ServiceResult OnReadCounter(
        ISystemContext context,
        NodeState node,
        NumericRange indexRange,
        QualifiedName dataEncoding,
        ref object value,
        ref StatusCode statusCode,
        ref DateTime timestamp)
    {
        value = _counter;
        statusCode = StatusCodes.Good;
        timestamp = DateTime.UtcNow;
        return ServiceResult.Good;
    }

    private void OnUpdateCounter(object state)
    {
        _counter++;
        if (state is BaseDataVariableState variable)
        {
            variable.Value = _counter;
            variable.Timestamp = DateTime.UtcNow;
            variable.ClearChangeMasks(SystemContext, false);
        }
    }

    // Add dynamic node under Device folder
    public BaseDataVariableState AddDynamicNode(string name, double initialValue)
    {
        var variable = new BaseDataVariableState(_deviceFolder)
        {
            SymbolicName = name,
            NodeId = new NodeId(Guid.NewGuid(), NamespaceIndex), // Unique NodeId
            BrowseName = new QualifiedName(name, NamespaceIndex),
            DisplayName = new LocalizedText("en-US", name),
            TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
            DataType = DataTypeIds.Double,
            ValueRank = ValueRanks.Scalar,
            AccessLevel = AccessLevels.CurrentReadOrWrite,
            UserAccessLevel = AccessLevels.CurrentReadOrWrite,
            Value = initialValue,
            StatusCode = StatusCodes.Good,
            Timestamp = DateTime.UtcNow
        };

        _deviceFolder.AddChild(variable);
        AddPredefinedNode(SystemContext, variable);

        // Notify clients (like Matrikon)
        _deviceFolder.ClearChangeMasks(SystemContext, true);

        Console.WriteLine($"[+] Dynamic node '{name}' added with NodeId {variable.NodeId}");
        return variable;
    }

    // Update value of dynamic node
    public void UpdateNodeValue(string name, double newValue)
    {
        var children = new List<BaseInstanceState>();
        _deviceFolder.GetChildren(SystemContext, children);

        foreach (var child in children)
        {
            if (child.DisplayName.Text == name && child is BaseDataVariableState variable)
            {
                variable.Value = newValue;
                variable.Timestamp = DateTime.UtcNow;
                variable.ClearChangeMasks(SystemContext, false);
                Console.WriteLine($"Child: {child.DisplayName}, NodeId: {child.NodeId}");

                Console.WriteLine($"[*] Updated node '{name}' to {newValue}");
                return;
            }
        }

        Console.WriteLine($"[!] Node '{name}' not found");
    }
}



               


