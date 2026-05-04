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

    private bool cardsLocked;


    private void Awake()
    {
        if (combat == null) combat = FindFirstObjectByType<CombatManager>();
        if (handView == null) handView = FindFirstObjectByType<HandView>();
    }

    public void OnCardClicked(CardInstance card)
    {
        if (card == null) return;
        
        if(cardsLocked) return;

        if (mode == InteractionMode.PlayMode)
        {
            if (combat == null)
            {
                Debug.LogError("HandInteractionController: No CombatManager assigned.");
                return;
            }
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

    // Now delegate selection state to CombatManager and sync visuals from there
    private void ToggleSelect(CardInstance card)
    {
        if (combat == null || combat.playerHand == null || handView == null)
        {
            // fallback to local behavior if combat missing
            InternalToggleLocal(card);
            return;
        }

        // let combat toggle its indices
        combat.ToggleSelectForCutInstance(card);

        // sync visuals / local list from combat state
        SyncSelectionFromCombat();
    }

    // fallback local toggle (keeps previous behavior if no CombatManager)
    private void InternalToggleLocal(CardInstance card)
    {
        if (selected.Contains(card))
        {
            selected.Remove(card);
            handView?.SetSelected(card, false);
            return;
        }

        if (selected.Count >= 2)
        {
            var old = selected[0];
            selected.RemoveAt(0);
            handView?.SetSelected(old, false);
        }

        selected.Add(card);
        handView?.SetSelected(card, true);
    }

    private void SyncSelectionFromCombat()
    {
        selected.Clear();
        if (combat == null || combat.playerHand == null || handView == null) return;

        foreach (var inst in combat.playerHand.cards)
        {
            bool isSel = combat.IsInstanceSelected(inst);
            handView.SetSelected(inst, isSel);
            if (isSel && inst != null) selected.Add(inst);
        }
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
        if (cutA != null) handView?.SetCutLines(cutA, false);
        if (cutB != null) handView?.SetCutLines(cutB, false);
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
        handView?.SetCutLines(cutA, true);
        handView?.SetCutLines(cutB, true);
    }


    public void ClearSelection()
    {
        // clear visuals
        foreach (var c in selected)
            handView?.SetSelected(c, false);

        selected.Clear();

        // also clear combat indices if present
        combat?.ClearSelectionPublic();
    }
    public void OnCutAxisClicked(CardInstance clickedCard, CutLineInput.Axis axis)
    {
        if (mode != InteractionMode.CutAxisMode) return;

        if (clickedCard != cutA && clickedCard != cutB) return;

        if (combat == null) return;

        if (axis == CutLineInput.Axis.Horizontal)
            combat.ExecuteCutHorizontal(cutA, cutB);
        else
            combat.ExecuteCutVertical(cutA, cutB);

        HideCutLines();
        ClearSelection();

        mode = InteractionMode.PlayMode;

        if (handView != null && combat.playerHand != null)
            handView.Rebuild(combat.playerHand.cards);
    }
    
    public void SetCardsLocked(bool locked)
    {
        cardsLocked = locked;
        if (cardsLocked)
        {
            HideCutLines();
            ClearSelection();
            mode = InteractionMode.PlayMode;
        }
    }


}
