using System;

public class CardInstance
{
    public event Action OnChanged;

    public void NotifyChanged()
    {
        OnChanged?.Invoke();
    }
    
    public CardCorner topLeft;
    public CardCorner topRight;
    public CardCorner bottomLeft;
    public CardCorner bottomRight;

    public CardDefinition sourceDefinition;

    public CardInstance(CardDefinition def)
    {
        sourceDefinition = def;
        topLeft = def.topLeft.Clone();
        topRight = def.topRight.Clone();
        bottomLeft = def.bottomLeft.Clone();
        bottomRight = def.bottomRight.Clone();
    }

    public string GetName()
    {
        string a = bottomLeft?.namePartText ?? "";
        string b = bottomRight?.namePartText ?? "";
        return (a + " " + b).Trim();
    }

    // Silber zählt in jede Farbsumme rein
    public int GetSum(CardColors color)
    {
        int sum = 0;
        AddCorner(ref sum, topLeft, color);
        AddCorner(ref sum, topRight, color);
        AddCorner(ref sum, bottomLeft, color);
        AddCorner(ref sum, bottomRight, color);
        return sum;
    }

    private static void AddCorner(ref int sum, CardCorner c, CardColors queryColor)
    {
        if (c == null) return;

        bool matches =
            c.color == queryColor ||
            c.color == CardColors.Silver; // <- Silber zählt immer mit

        if (matches) sum += c.value;
    }
}