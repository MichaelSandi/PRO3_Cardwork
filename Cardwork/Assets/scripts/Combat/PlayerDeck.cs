using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerDeck : MonoBehaviour
{
    public CardLibrary cardLibrary;

    [Header("Deck Setup")]
    public CardDefinition[] startingDeckDefinitions;
    private CardInstance[] startingDeck;

    private Stack<CardInstance> drawPile = new();
    private List<CardInstance> discardPile = new();

    [SerializeField] private TextMeshProUGUI drawPileText;
    [SerializeField] private TextMeshProUGUI discardPileText;

    private void Awake()
    {
        if (cardLibrary == null) cardLibrary = FindFirstObjectByType<CardLibrary>();

        // startingDeck = new CardInstance[startingDeckDefinitions.Length];
        //
        // // Initialize startingDeck as CardInstances
        // for (int i = 0; i < startingDeckDefinitions.Length; i++)
        // {
        //     startingDeck[i] = new CardInstance(startingDeckDefinitions[i]);
        // }
    }

    private void Start()
    {
        // foreach (var cardDefinition in startingDeckDefinitions)
        // {
        //     var cardInstance = new CardInstance(cardDefinition);
        //     Array.Resize(ref startingDeck, startingDeck.Length + 1);
        //     startingDeck[startingDeck.Length - 1] = cardInstance;
        // }
    }

    private void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;
        if (kb.dKey.wasPressedThisFrame)
        {
            Debug.Log($"Drawpile contents: {drawPile.Count} cards. Discardpile contents: {discardPile.Count} cards.");
        }
    }

    // public void BuildStartingDeck()
    // {
    //     drawPile.Clear();
    //     discardPile.Clear();
    //
    //     if (cardLibrary == null || cardLibrary.allCards.Count == 0)
    //     {
    //         Debug.LogError("CardLibrary missing or empty.");
    //         return;
    //     }
    //
    //     // Wichtig: Start-Deck besteht aus *Instanzen* (nicht Definitions)
    //     for (int i = 0; i < startingDeck.Length; i++)
    //     {
    //         drawPile.Add(startingDeck[i]); // <- Instance erzeugen
    //     }
    //
    //     Shuffle(drawPile);
    // }
    
    public void SetCards(List<CardInstance> cards)
    {
        ClearAll();

        if (cards == null || cards.Count == 0)
            return;

        // mischen (Fisher-Yates)
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }

        // in DrawPile legen
        for (int i = 0; i < cards.Count; i++)
            drawPile.Push(cards[i]);
    }


    public CardInstance DrawOne()
    {
        if (drawPile.Count == 0)
        {
            if (discardPile.Count == 0)
                return null;

            // discard → draw reshuffle
            for (int i = 0; i < discardPile.Count; i++)
                drawPile.Push(discardPile[i]);

            discardPile.Clear();
        }

        RefreshUI();

        return drawPile.Pop();
    }
    
    
    // private void DrawPilePushTop(CardInstance card)
    // {
    //     if (card == null) return;
    //     drawPile.Insert(0, card);
    // }
    

    public void Discard(CardInstance card)
    {
        if (card == null) return;
        discardPile.Add(card);
        RefreshUI();
    }


    public void PutBackOnTop(CardInstance card)
    {
        drawPile.Push(card);
        RefreshUI();
    }
    

    public void ClearAll()
    {
        drawPile.Clear();
        discardPile.Clear();
        RefreshUI();
    }
    
    
    void RefreshUI()
    {
        if (drawPileText != null)
            drawPileText.text = $"Draw: {drawPile.Count}";

        if (discardPileText != null)
            discardPileText.text = $"Discard: {discardPile.Count}";
    }


}
