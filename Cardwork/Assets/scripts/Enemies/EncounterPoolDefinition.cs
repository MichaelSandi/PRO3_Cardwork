using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Encounter Pool Definition")]
public class EncounterPoolDefinition : ScriptableObject
{
    public List<EncounterDefinition> encounters = new();

    public EncounterDefinition PickRandom(System.Random rng)
    {
        if (encounters == null || encounters.Count == 0) return null;
        int idx = rng.Next(0, encounters.Count);
        return encounters[idx];
    }
}