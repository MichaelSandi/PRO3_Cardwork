using UnityEngine;
using UnityEngine.InputSystem;


public enum CombatState
{
    EnemyChooseIntent,
    PlayerDraw,
    PlayerPlayCard,
    EnemyExecuteIntent,
    EndRound
}

public class CombatManager : MonoBehaviour
{
    public HandView handView;

    public PlayerEntity player;
    public EnemyEntity enemy;

    private CombatState state;
    public CombatState State => state;

    public EnemyController enemyController;
    private EnemyMove currentIntent;

    public PlayerDeck playerDeck;
    public PlayerHand playerHand;

    private int selectedA = -1;
    private int selectedB = -1;

    [SerializeField] private int startHandSize = 3;

    [SerializeField] private int baseCutsPerRound = 1; // später durch Talismane modifizieren
    
    private int cutsRemainingThisRound;
    public int CutsRemainingThisRound => cutsRemainingThisRound;

    private int GetCutsPerRound()
    {
        // Später: baseCutsPerRound + talismanBonusCuts
        return Mathf.Max(0, baseCutsPerRound);
    }

    private void ClearSelection()
    {
        selectedA = -1;
        selectedB = -1;
    }


    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerEntity>();
        if (enemy == null) enemy = FindFirstObjectByType<EnemyEntity>();
        if (enemyController == null) enemyController = FindFirstObjectByType<EnemyController>();
        if (playerDeck == null) playerDeck = FindFirstObjectByType<PlayerDeck>();
        if (playerHand == null) playerHand = FindFirstObjectByType<PlayerHand>();
        if (handView == null) handView = FindFirstObjectByType<HandView>();

    }

    private void Start()
    {
        player.ResetForCombat();
        enemy.ResetForCombat();
        
        // 1) Events zuerst verbinden
        // playerHand.OnCardAdded += handView.AddCard;
        // playerHand.OnCardRemoved += handView.RemoveCard;
        playerHand.OnHandChanged += () => handView.Rebuild(playerHand.cards);
        handView.Rebuild(playerHand.cards); // initial sync


        // 2) Deck bauen
        playerDeck.BuildStartingDeck();
        
        // 3) Start-Hand ziehen (feuert Events -> Views spawnen)
        DrawStartingHand();
        
        // 4) Combat starten
        state = CombatState.EnemyChooseIntent;
        //Debug.Log("Combat started.");
        
        AdvanceState();
    }

    private void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;

        if (state == CombatState.PlayerPlayCard)
        {
            if (kb == null) return;

            // 1) Auswahl per 1..7 togglen (für Cutting)
            int pressedIndex = -1;
            if (kb.digit1Key.wasPressedThisFrame) pressedIndex = 0;
            else if (kb.digit2Key.wasPressedThisFrame) pressedIndex = 1;
            else if (kb.digit3Key.wasPressedThisFrame) pressedIndex = 2;
            else if (kb.digit4Key.wasPressedThisFrame) pressedIndex = 3;
            else if (kb.digit5Key.wasPressedThisFrame) pressedIndex = 4;
            else if (kb.digit6Key.wasPressedThisFrame) pressedIndex = 5;
            else if (kb.digit7Key.wasPressedThisFrame) pressedIndex = 6;

            if (pressedIndex != -1)
                ToggleSelectForCut(pressedIndex);

            // 2) Cut ausführen, wenn 2 ausgewählt
            if (kb.hKey.wasPressedThisFrame)
                TryCut(horizontal: true);

            if (kb.vKey.wasPressedThisFrame)
                TryCut(horizontal: false);

            // 3) Karte spielen: SPACE spielt "selectedA"
            if (kb.spaceKey.wasPressedThisFrame)
            {
                if (selectedA == -1)
                {
                    Debug.Log("No card selected to play. Select a card (1..7), then press SPACE.");
                }
                else
                {
                    TryPlayCardAt(selectedA);
                    ClearSelection(); // nach dem Spielen Selection reset
                }
            }
        }
    }

    private void DrawStartingHand()
    {
        Debug.Log($"Drawing starting hand of {startHandSize} cards.");

        for (int i = 0; i < startHandSize; i++)
        {
            var card = playerDeck.DrawOne();
            if (card != null)
                playerHand.Add(card);
        }
    }


    private void AdvanceState()
    {
        Debug.Log($"--- STATE: {state} ---");

        switch (state)
        {
            case CombatState.EnemyChooseIntent:
                HandleEnemyChooseIntent();
                break;

            case CombatState.PlayerDraw:
                HandlePlayerDraw();
                break;

            case CombatState.PlayerPlayCard:
                HandlePlayerPlayCard();
                break;

            case CombatState.EnemyExecuteIntent:
                HandleEnemyExecuteIntent();
                break;

            case CombatState.EndRound:
                HandleEndRound();
                break;
        }
    }

    private void HandleEnemyChooseIntent()
    {
        currentIntent = enemyController.ChooseIntent();
        Debug.Log($"Enemy intent: {currentIntent.moveName} ({currentIntent.damage} dmg)");

        state = CombatState.PlayerDraw;
        AdvanceState();
    }


    private void HandlePlayerDraw()
    {
        var card = playerDeck.DrawOne();
        if (card == null)
        {
            state = CombatState.PlayerPlayCard;
            AdvanceState();
            return;
        }

        bool added = playerHand.Add(card);
        if (!added)
        {
            // Hand voll → Karte zurück ins DrawPile
            playerDeck.PutBackOnTop(card);
            Debug.Log("Returned card to top of draw pile.");
        }

        state = CombatState.PlayerPlayCard;
        AdvanceState();
    }


    private void HandlePlayerPlayCard()
    {
        cutsRemainingThisRound = GetCutsPerRound();
        Debug.Log($"Cuts available this round: {cutsRemainingThisRound}");
        Debug.Log("Select cards with 1..7. Press H/V to cut. Press SPACE to play selected card.");

        playerHand.DebugPrintHand();
    }


    private void HandleEnemyExecuteIntent()
    {
        enemyController.ExecuteIntent(player);

        state = CombatState.EndRound;
        AdvanceState();
    }


    private void HandleEndRound()
    {
        Debug.Log("End of round checks");

        if (player.IsDead())
        {
            Debug.Log("PLAYER DIED – GAME OVER");
            return;
        }

        if (enemy.IsDead())
        {
            Debug.Log("ENEMY DIED – YOU WIN");
            return;
        }

        state = CombatState.EnemyChooseIntent;
        AdvanceState();
    }

    // Wird später von Input / CardPlay aufgerufen
    public void OnPlayerPlayedCard()
    {
        state = CombatState.EnemyExecuteIntent;
        AdvanceState();
    }

    private void TryPlayCardAt(int index)
    {
        var card = playerHand.GetAt(index);
        if (card == null)
        {
            Debug.Log("No card at that slot.");
            return;
        }

        Debug.Log($"Playing card: {card.GetName()}");

        // Effekte ausführen
        CardResolver.Play(card, player, enemy);

        // Blaues Draw nachziehen
        int draw = CardResolver.GetBlueDraw(card);
        // for (int i = 0; i < draw; i++)
        // {
        //     var extra = playerDeck.DrawOne();
        //     if (extra == null) break;
        //
        //     bool added = playerHand.Add(extra);
        //     if (!added)
        //     {
        //         playerDeck.PutBackOnTop(extra);
        //         Debug.Log("Blue draw failed – hand full, card returned to deck.");
        //         break;
        //     }
        // }

        // Discard + aus Hand entfernen
        playerDeck.Discard(card);
        playerHand.Remove(card);

        // Turn weiter
        OnPlayerPlayedCard();
    }
    
    public void TryPlayCard(CardInstance card)
    {
        if (state != CombatState.PlayerPlayCard)
        {
            Debug.Log("Cannot play card right now (wrong state).");
            return;
        }

        if (card == null) return;

        // Sicherstellen, dass die Karte wirklich in der Hand ist
        int index = playerHand.cards.IndexOf(card);
        if (index == -1)
        {
            Debug.LogWarning("Tried to play a card that is not in hand.");
            return;
        }

        // Nutze deine bestehende Logik, aber mit Index oder direkt Card
        TryPlayCardAt(index); // wenn das existiert und sauber ist
    }


    private void ToggleSelectForCut(int index)
    {
        if (playerHand.GetAt(index) == null)
        {
            Debug.Log("No card in that slot.");
            return;
        }

        // Wenn erneut gedrückt: abwählen
        if (selectedA == index)
        {
            selectedA = selectedB;
            selectedB = -1;
            Debug.Log($"Unselected slot {index + 1}. Selection now: {SelectionString()}");
            return;
        }

        if (selectedB == index)
        {
            selectedB = -1;
            Debug.Log($"Unselected slot {index + 1}. Selection now: {SelectionString()}");
            return;
        }

        // Reinwählen
        if (selectedA == -1)
            selectedA = index;
        else if (selectedB == -1)
            selectedB = index;
        else
        {
            // Schon 2 ausgewählt: ersetze die älteste (A)
            selectedA = selectedB;
            selectedB = index;
        }

        Debug.Log($"Selected slot {index + 1}. Selection now: {SelectionString()}");
    }

    private string SelectionString()
    {
        string a = selectedA == -1 ? "-" : (selectedA + 1).ToString();
        string b = selectedB == -1 ? "-" : (selectedB + 1).ToString();
        return $"[{a}, {b}]";
    }

    private void TryCut(bool horizontal)
    {
        if (cutsRemainingThisRound <= 0)
        {
            Debug.Log("No cuts remaining this round.");
            return;
        }

        if (selectedA == -1 || selectedB == -1)
        {
            Debug.Log("Select TWO cards (1..7) before cutting.");
            return;
        }

        var cardA = playerHand.GetAt(selectedA);
        var cardB = playerHand.GetAt(selectedB);

        if (cardA == null || cardB == null)
        {
            Debug.Log("One of the selected slots is empty.");
            return;
        }

        // Cut ausführen
        Debug.Log($"CUT {(horizontal ? "HORIZONTAL" : "VERTICAL")} between " +
                  $"{cardA.GetName()} (slot {selectedA + 1}) and {cardB.GetName()} (slot {selectedB + 1})");

        if (horizontal) CardCutter.CutHorizontal(cardA, cardB);
        else CardCutter.CutVertical(cardA, cardB);

        cutsRemainingThisRound--;
        Debug.Log($"Cut done. Cuts remaining this round: {cutsRemainingThisRound}");

        Debug.Log($"After cut: Slot {selectedA + 1} = {cardA.GetName()} | Slot {selectedB + 1} = {cardB.GetName()}");
        playerHand.DebugPrintHand();
        
        //handView.RefreshCard(cardA);
        //handView.RefreshCard(cardB);
        //playerHand.NotifyChanged();
    }
    

    public void ExecuteCutHorizontal(CardInstance a, CardInstance b)
    {
        if (cutsRemainingThisRound <= 0) return;

        CardCutter.CutHorizontal(a, b);

        cutsRemainingThisRound--;
        // wenn ihr CardInstance.OnChanged nutzt, ruft CutHorizontal ggf. NotifyChanged auf
    }

    public void ExecuteCutVertical(CardInstance a, CardInstance b)
    {
        if (cutsRemainingThisRound <= 0) return;

        CardCutter.CutVertical(a, b);

        cutsRemainingThisRound--;
    }

}