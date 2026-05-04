using System.Collections.Generic;

public static class MapGenerator
{
    public static RunMapState Generate(
        MapLayoutDefinition layout,
        EncounterPoolDefinition easy,
        EncounterPoolDefinition medium,
        EncounterPoolDefinition hard,
        EncounterDefinition fixedStartFight,
        EncounterDefinition fixedBossFight,
        CardPoolDefinition cardPool,
        MapGenerationConfig cfg,
        int seed)
    {
        var rng = new System.Random(seed);
        var state = new RunMapState();

        string startId = layout.GetStartNodeId();
        string endId = layout.GetEndNodeId();
        state.currentNodeId = startId;

        // init node runtime data
        for (int i = 0; i < layout.nodes.Count; i++)
        {
            var n = layout.nodes[i];
            state.nodes[n.id] = new NodeRuntimeData(n.id);
        }

        // compute depth (distance from start) for difficulty
        var depth = ComputeDepths(layout, startId);

        // assign data
        for (int i = 0; i < layout.nodes.Count; i++)
        {
            var n = layout.nodes[i];
            var d = state.nodes[n.id];

            d.reachable = depth[n.id] != 999;

            // Unreachable = dauerhaft locked (optional: verstecken)
            if (!d.reachable)
            {
                d.status = MapNodeStatus.Locked;
                //d.type = MapNodeType.Fight; // egal; view kann unreachable grau machen/ausblenden
                continue;
            }

            if (n.isStart)
            {
                d.type = MapNodeType.StartFight;
                d.encounter = fixedStartFight;
                continue;
            }

            if (n.isEnd)
            {
                d.type = MapNodeType.BossFight;
                d.encounter = fixedBossFight;
                continue;
            }

            if (!n.isPOI)
            {
                d.type = MapNodeType.Fight;
                d.encounter = PickEncounterByDepth(depth[n.id], easy, medium, hard, rng);
                continue;
            }

            var rolled = RollType(cfg, rng);
            d.type = rolled;

            if (rolled == MapNodeType.Fight)
                d.encounter = PickEncounterByDepth(depth[n.id], easy, medium, hard, rng);
            else if (rolled == MapNodeType.Chest)
            {
                int count = rng.Next(cfg.chestOptionsMin, cfg.chestOptionsMax + 1);
                d.chestOptions = cardPool != null ? cardPool.PickUnique(rng, count) : new List<CardDefinition>();
            }
            else if (rolled == MapNodeType.Mystery)
            {
                d.revealed = false;
                d.resolvedType = MapNodeType.Mystery;
            }
        }

        // Progress init: alles locked, aber unreachable bleibt locked
        foreach (var kv in state.nodes)
        {
            if (!kv.Value.reachable) continue;
            kv.Value.status = MapNodeStatus.Locked;
        }

        // // Start ist cleared und unlockt outgoing
        // if (state.Get(startId) != null && state.Get(startId).reachable)
        // {
        //     state.Get(startId).status = MapNodeStatus.Cleared;
        //     UnlockOutgoing(layout, state, startId);
        // }
        state.currentNodeId = null; // wir sind noch auf keinem Node "gestanden"
        var start = state.Get(startId);
        if (start != null && start.reachable)
        {
            start.status = MapNodeStatus.Available; // Start-Fight muss klickbar sein
        }


        return state;
    }
    
    

    public static void ResolveMysteryOnEnter(
        RunMapState state,
        string nodeId,
        int nodeDepth,
        EncounterPoolDefinition easy,
        EncounterPoolDefinition medium,
        EncounterPoolDefinition hard,
        CardPoolDefinition cardPool,
        MapGenerationConfig cfg,
        System.Random rng)
    {
        var d = state.Get(nodeId);
        if (d == null) return;
        if (d.type != MapNodeType.Mystery) return;
        if (d.revealed) return;

        // roll resolved type (exclude Mystery to avoid recursion)
        var resolved = RollType(cfg, rng, allowMystery: false);
        d.revealed = true;
        d.resolvedType = resolved;

        if (resolved == MapNodeType.Fight)
        {
            d.encounter = PickEncounterByDepth(nodeDepth, easy, medium, hard, rng);
        }
        else if (resolved == MapNodeType.Campfire)
        {
            d.campfireHealAmount = 0;
        }
        else if (resolved == MapNodeType.Chest)
        {
            int count = rng.Next(cfg.chestOptionsMin, cfg.chestOptionsMax + 1);
            d.chestOptions = cardPool != null ? cardPool.PickUnique(rng, count) : new List<CardDefinition>();
        }
    }

    public static void MarkClearedAndUnlockNext(MapLayoutDefinition layout, RunMapState state, string clearedNodeId)
    {
        var d = state.Get(clearedNodeId);
        if (d != null && d.reachable) d.status = MapNodeStatus.Cleared;
        UnlockOutgoing(layout, state, clearedNodeId);
    }

    private static void UnlockOutgoing(MapLayoutDefinition layout, RunMapState state, string fromNodeId)
    {
        // ALLE Available wieder sperren
        foreach (var kv in state.nodes)
        {
            if (kv.Value.status == MapNodeStatus.Available)
                kv.Value.status = MapNodeStatus.Locked;
        }

        // NUR direkte Nachfolger freischalten
        var outs = layout.GetOutgoing(fromNodeId);
        for (int i = 0; i < outs.Count; i++)
        {
            var next = state.Get(outs[i]);
            if (next == null) continue;
            if (!next.reachable) continue;

            next.status = MapNodeStatus.Available;
        }
    }

    private static MapNodeType RollType(MapGenerationConfig cfg, System.Random rng, bool allowMystery = true)
    {
        float wf = cfg.weightFight;
        float wc = cfg.weightCampfire;
        float wch = cfg.weightChest;
        float wm = allowMystery ? cfg.weightMystery : 0f;

        float total = wf + wc + wch + wm;
        if (total <= 0f) return MapNodeType.Fight;

        double roll = rng.NextDouble() * total;
        if (roll < wf) return MapNodeType.Fight;
        roll -= wf;
        if (roll < wc) return MapNodeType.Campfire;
        roll -= wc;
        if (roll < wch) return MapNodeType.Chest;
        return MapNodeType.Mystery;
    }

    private static EncounterDefinition PickEncounterByDepth(int depth, EncounterPoolDefinition easy, EncounterPoolDefinition medium, EncounterPoolDefinition hard, System.Random rng)
    {
        if (depth <= 2) return easy != null ? easy.PickRandom(rng) : null;
        if (depth <= 5) return medium != null ? medium.PickRandom(rng) : null;
        return hard != null ? hard.PickRandom(rng) : null;
    }

    private static Dictionary<string, int> ComputeDepths(MapLayoutDefinition layout, string startId)
    {
        var depth = new Dictionary<string, int>();
        var q = new Queue<string>();

        depth[startId] = 0;
        q.Enqueue(startId);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            int d = depth[cur];

            var outs = layout.GetOutgoing(cur);
            for (int i = 0; i < outs.Count; i++)
            {
                var nxt = outs[i];
                if (!depth.ContainsKey(nxt))
                {
                    depth[nxt] = d + 1;
                    q.Enqueue(nxt);
                }
            }
        }

        // nodes unreachable from start default to large depth
        for (int i = 0; i < layout.nodes.Count; i++)
        {
            var id = layout.nodes[i].id;
            if (!depth.ContainsKey(id)) depth[id] = 999;
        }

        return depth;
    }
}
