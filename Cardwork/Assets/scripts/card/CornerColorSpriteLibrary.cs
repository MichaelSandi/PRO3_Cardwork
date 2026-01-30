using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Corner Color Sprite Library")]
public class CornerColorSpriteLibrary : ScriptableObject
{
    [SerializeField] private CornerColorSpriteSet[] spriteSets;

    private Dictionary<CardColors, CornerColorSpriteSet> lookup;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<CardColors, CornerColorSpriteSet>();

        foreach (var set in spriteSets)
        {
            if (set == null) continue;
            lookup[set.color] = set;
        }
    }

    public Sprite GetSprite(CardColors color, CornerPosition pos)
    {
        if (lookup == null || lookup.Count == 0)
            BuildLookup();

        if (lookup.TryGetValue(color, out var set))
            return set.GetSprite(pos);

        Debug.LogWarning($"No sprite set found for color {color}");
        return null;
    }
}