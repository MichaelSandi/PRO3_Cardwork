using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("Refs")] [SerializeField] private MapLayoutDefinition layout;
    [SerializeField] private MapGenerationConfig genConfig;
    [SerializeField] private EncounterPoolDefinition easyPool;
    [SerializeField] private EncounterPoolDefinition mediumPool;
    [SerializeField] private EncounterPoolDefinition hardPool;
    [SerializeField] private EncounterDefinition fixedStartFight;
    [SerializeField] private EncounterDefinition fixedBossFight;
    [SerializeField] private CardPoolDefinition cardPool;

    [Header("Worldspace View")] [SerializeField]
    private MapNodeView nodePrefab;

    [SerializeField] private Transform nodeParent;
    // [SerializeField] private int mapClickableLayer = 0; // optional info

    private readonly Dictionary<string, MapNodeView> views = new();

    private void OnEnable()
    {
        //Debug.Log("OnEnable Triggered: MapController initializing map.");
        EnsureRunMapState();
        BuildViews();
        RefreshAll();
    }

    private void EnsureRunMapState()
    {
        if (RunManager.Instance == null)
        {
            Debug.LogError("RunManager missing!");
            return;
        }

        if (RunManager.Instance.mapState == null)
        {
            int seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            RunManager.Instance.mapSeed = seed;

            RunManager.Instance.mapState = MapGenerator.Generate(
                layout,
                easyPool, mediumPool, hardPool,
                fixedStartFight, fixedBossFight,
                cardPool,
                genConfig,
                seed
            );
        }
    }

    private void BuildViews()
    {
        if (layout == null || nodePrefab == null || nodeParent == null) return;

        // clear old
        foreach (var kv in views)
            if (kv.Value != null)
                Destroy(kv.Value.gameObject);
        views.Clear();

        for (int i = 0; i < layout.nodes.Count; i++)
        {
            var n = layout.nodes[i];

            var v = Instantiate(nodePrefab, nodeParent);
            v.name = $"MapNode_{n.id}";
            v.transform.position = n.position;
            v.Init(n.id);

            // INPUT + COLLIDER am HitRoot (Child)
            Transform hit = v.HitRoot != null ? v.HitRoot : v.transform;

            // ensure input exists on hit object
            var input = hit.GetComponent<MapNodeInput>();
            if (input == null) input = hit.gameObject.AddComponent<MapNodeInput>();
            input.Bind(this, v);

            // ensure collider exists on hit object
            if (hit.GetComponent<Collider>() == null)
            {
                var col = hit.gameObject.AddComponent<BoxCollider>();
                col.size = new Vector3(1f, 1f, 0.2f);
            }

            views[n.id] = v;
        }
    }

    public void RefreshAll()
    {
        var state = RunManager.Instance.mapState;
        foreach (var kv in views)
        {
            var data = state.Get(kv.Key);
            kv.Value.Refresh(data);
        }
    }

    public void OnNodeClicked(string nodeId)
    {
        Debug.Log("OnNodeClicked: " + nodeId);
        // only allow in MapMode (optional guard)
        // if (GameFlowController.Instance != null && !GameFlowController.Instance.IsInMapMode)
        //     return;

        var state = RunManager.Instance.mapState;
        var data = state.Get(nodeId);
        if (data == null) return;

        if (!data.reachable) return;
        if (data.status != MapNodeStatus.Available) return;

        // Mystery resolves on enter (Icon swap)
        if (data.type == MapNodeType.Mystery && !data.revealed)
        {
            // Use a deterministic RNG from seed
            var rng = new System.Random(RunManager.Instance.mapSeed ^ nodeId.GetHashCode());
            int depth = ComputeDepth(nodeId);

            MapGenerator.ResolveMysteryOnEnter(
                state, nodeId, depth,
                easyPool, mediumPool, hardPool,
                cardPool, genConfig, rng
            );

            RefreshAll();
        }

        var displayed = data.GetDisplayedType();
        RunManager.Instance.currentNodeId = nodeId;

        switch (displayed)
        {
            case MapNodeType.Fight:
            case MapNodeType.StartFight:
            case MapNodeType.BossFight:
                if (data.encounter == null)
                {
                    Debug.LogError("Fight node has no encounter assigned.");
                    return;
                }

                GameFlowController.Instance.StartCombat(data.encounter);
                break;

            case MapNodeType.Campfire:
                ApplyCampfire(nodeId);
                break;

            case MapNodeType.Chest:
                ApplyChest(nodeId, data);
                break;
        }
    }

    private void ApplyCampfire(string nodeId)
    {
        int missing = Mathf.Max(0, RunManager.Instance.maxHp - RunManager.Instance.currentHp);
        int heal = Mathf.CeilToInt(missing * genConfig.healMissingHpPercent);
        if (heal < genConfig.healMinimum) heal = genConfig.healMinimum;

        RunManager.Instance.currentHp = Mathf.Min(RunManager.Instance.maxHp, RunManager.Instance.currentHp + heal);

        MapGenerator.MarkClearedAndUnlockNext(layout, RunManager.Instance.mapState, nodeId);
        RefreshAll();
    }

    private void ApplyChest(string nodeId, NodeRuntimeData data)
    {
        // MVP: auto pick first option
        if (data.chestOptions != null && data.chestOptions.Count > 0 && data.chestOptions[0] != null)
            RunManager.Instance.deck.Add(new CardInstance(data.chestOptions[0]));

        MapGenerator.MarkClearedAndUnlockNext(layout, RunManager.Instance.mapState, nodeId);
        RefreshAll();
    }

    private int ComputeDepth(string nodeId)
    {
        // For MVP: quick BFS
        var startId = layout.GetStartNodeId();
        if (nodeId == startId) return 0;

        var q = new Queue<string>();
        var dist = new Dictionary<string, int>();

        dist[startId] = 0;
        q.Enqueue(startId);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            int d = dist[cur];

            var outs = layout.GetOutgoing(cur);
            for (int i = 0; i < outs.Count; i++)
            {
                var nxt = outs[i];
                if (!dist.ContainsKey(nxt))
                {
                    dist[nxt] = d + 1;
                    q.Enqueue(nxt);
                }
            }
        }

        return dist.TryGetValue(nodeId, out int res) ? res : 999;
    }
}