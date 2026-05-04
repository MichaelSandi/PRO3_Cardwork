using System.Collections.Generic;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [Header("Player")]
    public int maxHp = 25;
    public int currentHp;

    [Header("Deck (persistent CardInstances)")]
    public List<CardInstance> deck = new();
    
    public RunMapState mapState;
    public string currentNodeId;
    public int mapSeed;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ===== Run Lifecycle =====

    public void StartNewRun(List<CardDefinition> starterDeck)
    {
        deck.Clear();

        foreach (var data in starterDeck)
        {
            deck.Add(new CardInstance(data));
        }

        currentHp = maxHp;
        
        mapState = null;
        currentNodeId = null;
        mapSeed = 0;

    }

    public void ResetRun()
    {
        deck.Clear();
        currentHp = maxHp;
    }
}