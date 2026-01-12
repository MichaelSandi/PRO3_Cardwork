using UnityEngine;

[CreateAssetMenu(menuName = "Cards/Card Definition")]
public class CardDefinition : ScriptableObject
{
    public string baseId; // optional, falls du intern IDs willst

    public CardCorner topLeft;
    public CardCorner topRight;
    public CardCorner bottomLeft;
    public CardCorner bottomRight;
}
