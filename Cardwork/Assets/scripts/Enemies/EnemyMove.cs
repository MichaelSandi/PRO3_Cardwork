using UnityEngine;
using System.Collections.Generic;

public enum EnemyEffectType
{
    DealDamagePlayer,
    GainArmorSelf,
    GainArmorAllEnemies,
    GainArmorRandomEnemy,
    BuffAlliesDamage,      // z.B. "Allies gain +X damage next attack"
    BuffRandomEnemyDamage,
    CloakSelf,        // untargetable for X turns
    HealSelf,
    HealRandomEnemy,
    HealAllEnemies,
    CleanseDebuffsSelf,
    DiscardPlayerCard  // force player to discard X cards
}


[System.Serializable]
public class EnemyMove
{
    public string moveName;

    [Range(0, 100)]
    public int weight = 1;

    // optional cooldown später
    // public int cooldownTurns;

    public List<EnemyMoveEffect> effects = new();
}

[System.Serializable]
public struct EnemyMoveEffect
{
    public EnemyEffectType type;
    //public EnemyTargetScope scope;
    public int amount;     // X
    public int duration;   // für cloak/taunt etc.
}
