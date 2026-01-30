using System.Collections.Generic;
using UnityEngine;

public class HandView : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private Transform cardParent;

    // Für schnelles Reuse: Instanz -> View
    private readonly Dictionary<CardInstance, CardView> views = new();

    public void Rebuild(IReadOnlyList<CardInstance> handOrder)
    {
        if (cardPrefab == null || cardParent == null || handOrder == null) return;

        // 1) Views entfernen, die nicht mehr in der Hand sind
        var toRemove = new List<CardInstance>();
        foreach (var kv in views)
        {
            if (!Contains(handOrder, kv.Key))
                toRemove.Add(kv.Key);
        }
        foreach (var inst in toRemove)
        {
            if (views.TryGetValue(inst, out var v) && v != null)
                Destroy(v.gameObject);
            views.Remove(inst);
        }

        // 2) Views erstellen, die fehlen
        for (int i = 0; i < handOrder.Count; i++)
        {
            var inst = handOrder[i];
            if (inst == null) continue;

            if (!views.ContainsKey(inst))
            {
                var v = Instantiate(cardPrefab, cardParent);
                v.SetFaceUp(true);
                v.Bind(inst);
                views[inst] = v;
            }
        }

        // 3) Reihenfolge im Hierarchy-Parent exakt nach HandOrder setzen + Positionen
        float spacing = 14f; // anpassen
        for (int i = 0; i < handOrder.Count; i++)
        {
            var inst = handOrder[i];
            if (inst == null) continue;

            var v = views[inst];
            if (v == null) continue;

            // siblings im Parent auf Hand-Reihenfolge setzen
            v.transform.SetSiblingIndex(i);
            v.transform.localPosition = new Vector3(i * spacing, 0f, 0f);

            // optional: refresh, falls sich values geändert haben (cuts)
            v.Refresh();
            v.SetSelected(false);
        }
    }

    private bool Contains(IReadOnlyList<CardInstance> list, CardInstance item)
    {
        for (int i = 0; i < list.Count; i++)
            if (ReferenceEquals(list[i], item))
                return true;
        return false;
    }
    
    public void SetSelected(CardInstance inst, bool selected)
    {
        if (inst == null) return;
        if (views.TryGetValue(inst, out var v) && v != null)
            v.SetSelected(selected);
        
    }
    
    public void SetCutLines(CardInstance inst, bool visible)
    {
        if (inst == null) return;
        if (views.TryGetValue(inst, out var v) && v != null)
            v.SetCutLinesVisible(visible);
    }


}
