using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeck : MonoBehaviour
{
    public CardLibrary cardLibrary;

    [Header("Deck Setup")]
    public int startingDeckSize = 10;

    private readonly List<CardInstance> drawPile = new();
    private readonly List<CardInstance> discardPile = new();

    private void Awake()
    {
        if (cardLibrary == null) cardLibrary = FindFirstObjectByType<CardLibrary>();
    }

    private void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb == null) return;
        if (kb.dKey.wasPressedThisFrame)
        {
            Debug.Log($"Drawpile contents: {drawPile.Count} cards.");
        }
    }

    public void BuildStartingDeck()
    {
        drawPile.Clear();
        discardPile.Clear();

        if (cardLibrary == null || cardLibrary.allCards.Count == 0)
        {
            Debug.LogError("CardLibrary missing or empty.");
            return;
        }

        // Wichtig: Start-Deck besteht aus *Instanzen* (nicht Definitions)
        for (int i = 0; i < startingDeckSize; i++)
        {
            var def = cardLibrary.allCards[UnityEngine.Random.Range(0, cardLibrary.allCards.Count)];
            drawPile.Add(new CardInstance(def)); // <- Instance erzeugen
        }

        Shuffle(drawPile);
        Debug.Log($"Built starting deck with {drawPile.Count} CardInstances.");
    }

    public CardInstance DrawOne()
    {
        if (drawPile.Count == 0)
        {
            ReshuffleDiscardIntoDraw();
            if (drawPile.Count == 0) return null;
        }

        // Draw existing Instance
        var card = drawPile[0];
        drawPile.RemoveAt(0);
        return card;
    }

    public void Discard(CardInstance card)
    {
        if (card == null) return;
        discardPile.Add(card); // <- gleiche Instanz landet im Discard
    }

    private void ReshuffleDiscardIntoDraw()
    {
        if (discardPile.Count == 0) return;

        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(drawPile);

        Debug.Log("Reshuffled discard into draw pile (instances preserved).");
    }

    private void Shuffle(List<CardInstance> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
    
    public void PutBackOnTop(CardInstance card)
    {
        if (card == null) return;
        drawPile.Insert(0, card);
    }

}
