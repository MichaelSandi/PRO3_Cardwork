using System;
using UnityEngine;
using UnityEngine.Serialization;

public enum CardColors
{
    Orange,
    Yellow,
    Pink,
    Blue
}
public enum EffectType
{
    None,
    Damage,
    Heal,
    Armor,
    DrawCard,
    DiscardCard,
    Mark,
    Burn,
    RandomDamage
}

[Serializable]
public class CardCorner
{
    public CardColors cornerColor;
    public int value;
    public EffectType effect;
    public CardColors effectColor;
    
    // Optional: Name-Teil, der aus dieser Ecke kommt
    public string namePart;

    public CardCorner Clone()
    {
        return new CardCorner
        {
            cornerColor = this.cornerColor,
            value = this.value,
            namePart = this.namePart
        };
    }
}
