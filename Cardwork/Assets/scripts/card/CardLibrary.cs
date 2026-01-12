using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CardLibrary : MonoBehaviour
{
    [Header("All Card Blueprints")] public List<CardDefinition> allCards = new List<CardDefinition>();

    // Wird im Inspector als Button angezeigt

#if UNITY_EDITOR
    [ContextMenu("Load All CardDefinitions")]
    public void LoadAllCards()
    {
        allCards.Clear();

        string[] guids = AssetDatabase.FindAssets("t:CardDefinition");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CardDefinition card = AssetDatabase.LoadAssetAtPath<CardDefinition>(path);
            if (card != null)
            {
                allCards.Add(card);
            }
        }

        Debug.Log("Loaded " + allCards.Count + " CardDefinitions into CardLibrary.");
#else
        Debug.LogWarning("LoadAllCards() funktioniert nur im Editor!");
#endif
    }

    // Runtime: neue Instanz ziehen
    public CardInstance GetCardInstance(int index)
    {
        if (index < 0 || index >= allCards.Count) return null;
        return new CardInstance(allCards[index]);
    }

    public CardInstance GetRandomCardInstance()
    {
        if (allCards.Count == 0) return null;
        int randomIndex = UnityEngine.Random.Range(0, allCards.Count);
        return GetCardInstance(randomIndex);
    }
}