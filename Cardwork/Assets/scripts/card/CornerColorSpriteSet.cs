using System;
using UnityEngine;

[Serializable]
public class CornerColorSpriteSet
{
    public CardColors color;

    public Sprite topLeft;
    public Sprite topRight;
    public Sprite bottomLeft;
    public Sprite bottomRight;

    public Sprite GetSprite(CornerPosition pos)
    {
        return pos switch
        {
            CornerPosition.TopLeft => topLeft,
            CornerPosition.TopRight => topRight,
            CornerPosition.BottomLeft => bottomLeft,
            CornerPosition.BottomRight => bottomRight,
            _ => null
        };
    }
}