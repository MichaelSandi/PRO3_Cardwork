using System.Collections.Generic;

public enum MapNodeType
{
    StartFight,
    BossFight,
    Fight,
    Campfire,
    Chest,
    Mystery
}

public enum MapNodeStatus
{
    Locked,
    Available,
    Cleared
}

public class NodeRuntimeData
{
    public string nodeId;

    public MapNodeType type;
    public MapNodeStatus status;

    public bool reachable; // <<< NEW: vom Start aus überhaupt erreichbar?

    // Mystery
    public bool revealed;
    public MapNodeType resolvedType;

    // Payload
    public EncounterDefinition encounter;
    public int campfireHealAmount;
    public List<CardDefinition> chestOptions;

    public NodeRuntimeData(string id)
    {
        nodeId = id;
        status = MapNodeStatus.Locked;
        reachable = true;
        revealed = false;
        resolvedType = MapNodeType.Mystery;
        chestOptions = new List<CardDefinition>();
    }

    public MapNodeType GetDisplayedType()
    {
        if (type != MapNodeType.Mystery) return type;
        return revealed ? resolvedType : MapNodeType.Mystery;
    }
}
