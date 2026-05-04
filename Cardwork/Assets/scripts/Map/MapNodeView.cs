using UnityEngine;

public class MapNodeView : MonoBehaviour
{
    [Header("Render")] 
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private GameObject availableOutline;
    [SerializeField] private GameObject clearedTint;
    [SerializeField] private GameObject lockedTint;
    [SerializeField] private GameObject unreachableTint; // optional

    [Header("Opacity")] [Range(0f, 1f)] [SerializeField]
    private float unreachableAlpha = 0.25f;

    [Header("Icons")] public Sprite iconFight;
    public Sprite iconCampfire;
    public Sprite iconChest;
    public Sprite iconMystery;
    public Sprite iconStart;
    public Sprite iconBoss;

    [SerializeField] private Transform hitRoot;
    public Transform HitRoot => hitRoot;

    public string NodeId { get; private set; }

    public void Init(string nodeId)
    {
        NodeId = nodeId;
    }

    public void Refresh(NodeRuntimeData data)
    {
        if (data == null) return;

        // Optional: unreachable = komplett ausblenden
        // gameObject.SetActive(data.reachable);

        if (unreachableTint != null) unreachableTint.SetActive(!data.reachable);

        var displayed = data.GetDisplayedType();
        if (iconRenderer != null) iconRenderer.sprite = PickIcon(displayed);

        bool isAvailable = data.status == MapNodeStatus.Available;
        bool isCleared = data.status == MapNodeStatus.Cleared;
        bool isLocked = data.status == MapNodeStatus.Locked;

        if (availableOutline != null) availableOutline.SetActive(isAvailable);
        if (clearedTint != null) clearedTint.SetActive(isCleared);
        if (lockedTint != null) lockedTint.SetActive(isLocked && data.reachable);
        
        // Base color
        Color c = iconRenderer.color;
        if(isLocked)
            iconRenderer.color = new Color(c.r, c.g, c.b, unreachableAlpha);
        
        if(isAvailable)
            iconRenderer.color = new Color(c.r, c.g, c.b, 1f);
        
        if(isCleared)
            iconRenderer.color = new Color(c.r, c.g, c.b, 0.7f);
        

        // if (data.status == MapNodeStatus.Cleared)
        //     iconRenderer.color = new Color(c.r, c.g, c.b, c.a * 0.7f);
    }

    private Sprite PickIcon(MapNodeType type)
    {
        return type switch
        {
            MapNodeType.StartFight => iconStart,
            MapNodeType.BossFight => iconBoss,
            MapNodeType.Fight => iconFight,
            MapNodeType.Campfire => iconCampfire,
            MapNodeType.Chest => iconChest,
            MapNodeType.Mystery => iconMystery,
            _ => iconMystery
        };
    }
}