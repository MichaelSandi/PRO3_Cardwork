using UnityEngine;

public class CardSpawnTest : MonoBehaviour
{
    public CardDefinition testDefinition;
    public CardView cardPrefab;

    private void Start()
    {
        var view = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity);
        var instance = new CardInstance(testDefinition);
        view.SetFaceUp(true);
        view.Bind(instance);
    }
}