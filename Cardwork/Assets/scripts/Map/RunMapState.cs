using System.Collections.Generic;

public class RunMapState
{
    public string currentNodeId;
    public Dictionary<string, NodeRuntimeData> nodes = new();

    public NodeRuntimeData Get(string nodeId)
    {
        if (nodeId == null) return null;
        nodes.TryGetValue(nodeId, out var data);
        return data;
    }
}