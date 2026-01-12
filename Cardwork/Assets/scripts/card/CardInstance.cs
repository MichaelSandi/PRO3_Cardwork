using UnityEngine;

public class CardInstance
{
    public CardCorner topLeft;
    public CardCorner topRight;
    public CardCorner bottomLeft;
    public CardCorner bottomRight;

    // optional: Referenz zum Template, falls benötigt
    public CardDefinition sourceDefinition;

    public CardInstance(CardDefinition definition)
    {
        sourceDefinition = definition;

        // Runtime-Kopien erstellen
        topLeft = definition.topLeft.Clone();
        topRight = definition.topRight.Clone();
        bottomLeft = definition.bottomLeft.Clone();
        bottomRight = definition.bottomRight.Clone();
    }

    // Name = Kombination aus den unteren Ecken
    public string GetName()
    {
        string a = bottomLeft.namePart;
        string b = bottomRight.namePart;

        if (string.IsNullOrEmpty(a)) a = "";
        if (string.IsNullOrEmpty(b)) b = "";

        return (a + " " + b).Trim();
    }

    // Summiert Werte aller Ecken einer bestimmten Farbe
    public int GetTotalValueByColor(CardColors color)
    {
        int total = 0;

        if (topLeft.cornerColor == color) total += topLeft.value;
        if (topRight.cornerColor == color) total += topRight.value;
        if (bottomLeft.cornerColor == color) total += bottomLeft.value;
        if (bottomRight.cornerColor == color) total += bottomRight.value;

        return total;
    }
}
