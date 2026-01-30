using System.Collections.Generic;
using UnityEngine;

public class HandInteractionController : MonoBehaviour
{
    public enum InteractionMode { PlayMode, CutSelectMode, CutAxisMode }

    [Header("Refs")]
    public CombatManager combat;
    public HandView handView;

    [Header("State")]
    public InteractionMode mode = InteractionMode.PlayMode;

    private readonly List<CardInstance> selected = new List<CardInstance>(2);
    
    private CardInstance cutA;
    private CardInstance cutB;


    private void Awake()
    {
        if (combat == null) combat = FindFirstObjectByType<CombatManager>();
        if (handView == null) handView = FindFirstObjectByType<HandView>();
    }

    public void OnCardClicked(CardInstance card)
    {
        if (card == null) return;

        if (mode == InteractionMode.PlayMode)
        {
            combat.TryPlayCard(card);
            return;
        }

        if (mode == InteractionMode.CutSelectMode)
        {
            ToggleSelect(card);
            return;
        }

        // CutAxisMode kommt später
    }

    private void ToggleSelect(CardInstance card)
    {
        // Wenn schon selected: deselect
        if (selected.Contains(card))
        {
            selected.Remove(card);
            handView.SetSelected(card, false);
            return;
        }

        // Wenn neu selected und schon 2 drin: älteste rauswerfen (UND VISUELL DESELECTEN!)
        if (selected.Count >= 2)
        {
            var old = selected[0];
            selected.RemoveAt(0);
            handView.SetSelected(old, false);
        }

        selected.Add(card);
        handView.SetSelected(card, true);
    }


    public int SelectedCount => selected.Count;

    public void EnterPlayMode()
    {
        mode = InteractionMode.PlayMode;
        HideCutLines();
        ClearSelection();
    }

    private void HideCutLines()
    {
        if (cutA != null) handView.SetCutLines(cutA, false);
        if (cutB != null) handView.SetCutLines(cutB, false);
        cutA = null;
        cutB = null;
    }


    public void EnterCutSelectMode()
    {
        mode = InteractionMode.CutSelectMode;
        ClearSelection();
    }
    
    public void ConfirmCutSelection()
    {
        if (selected.Count != 2) return;

        cutA = selected[0];
        cutB = selected[1];

        mode = InteractionMode.CutAxisMode;

        // Linien anzeigen (auf beiden Karten)
        handView.SetCutLines(cutA, true);
        handView.SetCutLines(cutB, true);
    }


    public void ClearSelection()
    {
        foreach (var c in selected)
            handView.SetSelected(c, false);

        selected.Clear();
    }
    
    
    public void OnCutAxisClicked(CardInstance clickedCard, CutLineInput.Axis axis)
    {
        if (mode != InteractionMode.CutAxisMode) return;

        // Nur auf den zwei confirmed Karten erlauben
        if (clickedCard != cutA && clickedCard != cutB) return;

        // Cut ausführen
        if (axis == CutLineInput.Axis.Horizontal)
            combat.ExecuteCutHorizontal(cutA, cutB);
        else
            combat.ExecuteCutVertical(cutA, cutB);

        // Linien weg, zurück in PlayMode
        HideCutLines();

        // Optional: selected outlines weg (damit es clean aussieht)
        ClearSelection();

        mode = InteractionMode.PlayMode;

        // View refreshen (je nachdem wie ihr es gelöst habt)
        handView.Rebuild(combat.playerHand.cards);
    }


}
