using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum CardColors
{
    Orange,
    Yellow,
    Pink,
    Blue,
    Silver //zählt für jede Farbe mit
}

public enum DamageDistribution
{
    OneEnemy, // Deal X to one enemy
    AllEnemies, // Deal X to all enemies
    SplitAmongAllEnemies, // Total X, random split (1er Schritte) among all enemies
    SplitAmongYRandomEnemies // Total X, random split among Y random target slots (with replacement)
}

public enum EffectType
{
    None,

    // Player
    RegainLife,
    GainArmor,
    DrawCards,
    DiscardCards,

    // Enemy status
    Marked,
    Burning,
    Weakened,

    // Damage
    DealDamage
}



[Serializable]
public class CardCorner
{
    public CardColors color;
    public int value;

    // Nur genutzt in BottomLeft/BottomRight
    public string namePartText;

    // Nur genutzt in BottomLeft/BottomRight
    public EffectModuleData effectModule;

    public CardCorner Clone()
    {
        return new CardCorner
        {
            color = color,
            value = value,
            namePartText = namePartText,
            effectModule = effectModule != null ? effectModule.Clone() : null
        };
    }
}



[Serializable]
public class EffectModuleData
{
    public EffectType type = EffectType.None;

    // X ist immer eine Farbsumme (inkl. Silber)
    public CardColors xColor = CardColors.Blue;

    // Nur relevant für DamageDistribution.SplitAmongYRandomEnemies
    public CardColors yColor = CardColors.Blue;

    // Nur relevant, wenn type == DealDamage
    public DamageDistribution damageDistribution = DamageDistribution.OneEnemy;

    // Optional: UI-Text override, falls du später fancy formatting willst.
    [TextArea] public string effectTextOverride;

    public EffectModuleData Clone()
    {
        return new EffectModuleData
        {
            type = type,
            xColor = xColor,
            yColor = yColor,
            damageDistribution = damageDistribution,
            effectTextOverride = effectTextOverride
        };
    }
}
