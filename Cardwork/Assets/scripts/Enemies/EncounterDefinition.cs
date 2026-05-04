using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Encounter Definition")]
public class EncounterDefinition : ScriptableObject
{
    public string encounterName = "Encounter";

    [Tooltip("Enemies spawned in order. If you have spawn points, index maps to spawn point index.")]
    public List<EnemyDefinition> enemies = new List<EnemyDefinition>();
}