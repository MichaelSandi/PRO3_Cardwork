using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerHand : MonoBehaviour
{
    // public event Action<CardInstance> OnCardAdded;
    // public event Action<CardInstance> OnCardRemoved;
    public event Action OnHandChanged;

    public bool HasSpace => cards.Count < handLimit;

    public List<CardInstance> cards = new List<CardInstance>();
    public int handLimit = 7;

    // public void NotifyChanged()
    // {
    //     OnHandChanged?.Invoke();
    // }

    
    public bool Add(CardInstance card)
    {
        
        if (cards.Count >= handLimit)
        {
            Debug.Log("Hand is full. Draw skipped.");
            return false;
        }

        cards.Add(card);
        //OnCardAdded?.Invoke(card);
        OnHandChanged?.Invoke();

        Debug.Log($"Drew card: {card.GetName()} | Hand size: {cards.Count}");
        DebugPrintHand();
        return true;
    }


    public CardInstance GetAt(int index)
    {
        if (index < 0 || index >= cards.Count) return null;
        return cards[index];
    }

    public void Remove(CardInstance card)
    {
        if (cards.Remove(card))
        {
            //OnCardRemoved?.Invoke(card);
            OnHandChanged?.Invoke();
            DebugPrintHand();
        }
    }

    public void DebugPrintHand()
    {
        string s = "HAND: ";
        for (int i = 0; i < cards.Count; i++)
            s += $"[{i+1}:{cards[i].GetName()}] ";
        Debug.Log(s);
    }
}