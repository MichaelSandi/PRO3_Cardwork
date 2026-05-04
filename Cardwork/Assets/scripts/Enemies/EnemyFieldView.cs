using System.Collections.Generic;
using UnityEngine;

public class EnemyFieldView : MonoBehaviour
{
    [SerializeField] private EnemyView enemyPrefab;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private float spacing = 3.0f;

    private readonly Dictionary<EnemyInstance, EnemyView> views = new();

    public void Rebuild(IReadOnlyList<EnemyInstance> enemies)
    {
        if (enemyPrefab == null || enemyParent == null || enemies == null) return;

        // remove missing
        var toRemove = new List<EnemyInstance>();
        foreach (var kv in views)
            if (!Contains(enemies, kv.Key))
                toRemove.Add(kv.Key);

        foreach (var inst in toRemove)
        {
            if (views.TryGetValue(inst, out var v) && v != null)
                Destroy(v.gameObject);
            views.Remove(inst);
        }

        // add missing
        for (int i = 0; i < enemies.Count; i++)
        {
            var inst = enemies[i];
            if (inst == null) continue;

            if (!views.ContainsKey(inst))
            {
                var v = Instantiate(enemyPrefab, enemyParent);
                v.Bind(inst);
                views[inst] = v;
            }
        }

        // order + position + refresh
        for (int i = 0; i < enemies.Count; i++)
        {
            var inst = enemies[i];
            if (inst == null) continue;

            var v = views[inst];
            v.transform.SetSiblingIndex(i);
            v.transform.localPosition = new Vector3(i * spacing, 0f, 0f);
            v.Refresh();
        }
    }

    public void RefreshAll()
    {
        foreach (var kv in views)
            if (kv.Value != null)
                kv.Value.Refresh();
    }

    public EnemyView GetView(EnemyInstance inst)
        => inst != null && views.TryGetValue(inst, out var v) ? v : null;

    private bool Contains(IReadOnlyList<EnemyInstance> list, EnemyInstance item)
    {
        for (int i = 0; i < list.Count; i++)
            if (ReferenceEquals(list[i], item))
                return true;
        return false;
    }
}
