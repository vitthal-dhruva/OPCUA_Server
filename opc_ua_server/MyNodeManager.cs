
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

        
    }

   

    // Add dynamic node under Device folder
    public BaseDataVariableState AddDynamicNode(string name, object initialValue, NodeId dataType)
    {
        var variable = new BaseDataVariableState(_deviceFolder)
        {
            SymbolicName = name,
            NodeId = new NodeId(Guid.NewGuid(), NamespaceIndex), // Unique NodeId
            BrowseName = new QualifiedName(name, NamespaceIndex),
            DisplayName = new LocalizedText("en-US", name),
            TypeDefinitionId = VariableTypeIds.BaseDataVariableType,
            DataType = dataType,
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



               


