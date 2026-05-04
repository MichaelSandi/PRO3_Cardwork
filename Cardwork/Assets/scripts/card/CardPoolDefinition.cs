using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Card Pool Definition")]
public class CardPoolDefinition : ScriptableObject
{
    public List<CardDefinition> cards = new();

    public List<CardDefinition> PickUnique(System.Random rng, int count)
    {
        var result = new List<CardDefinition>();
        if (cards == null || cards.Count == 0 || count <= 0) return result;

        // simple unique pick without replacement
        var temp = new List<CardDefinition>(cards);

        for (int i = 0; i < count && temp.Count > 0; i++)
        {
            int idx = rng.Next(0, temp.Count);
            result.Add(temp[idx]);
            temp.RemoveAt(idx);
        }
        return result;
    }
}