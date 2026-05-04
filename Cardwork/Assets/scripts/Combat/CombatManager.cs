using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;


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
    public EncounterDefinition firstEncounter;
    public EnemyFieldView enemyFieldView;

    private struct PendingCardPlay
    {
        public CardInstance card;
        public int nextModuleIndex;          // 0 oder 1: ab hier geht’s weiter
        public bool hasChosenTarget;
        public EnemyInstance chosenTarget;   // der eine Target für beide Module
    }
    
    private PendingCardPlay pending;

    
    private bool waitingForTarget = false;
    public bool IsWaitingForTarget => waitingForTarget;
    //private TargetRequest pendingRequest;
    private CardInstance pendingCard;

    [SerializeField] private HandInteractionController handInteraction;
    
    public GameObject targetVignetteUI;


    public readonly List<EnemyInstance> activeEnemies = new();

    private System.Random rng = new System.Random();

    public HandView handView;

    private CombatState state;
    public CombatState State => state;

    [Header("Refs")] public PlayerEntity player;
    public PlayerDeck playerDeck;
    public PlayerHand playerHand;

    private int selectedA = -1;
    private int selectedB = -1;

    [SerializeField] private int startHandSize = 3;

    [SerializeField] private int baseCutsPerRound = 1; // später durch Talismane modifizieren

    private int cutsRemainingThisRound;
    public int CutsRemainingThisRound => cutsRemainingThisRound;

    public void ToggleSelectForCutInstance(CardInstance card)
    {
        if (card == null || playerHand == null) return;
        int idx = playerHand.cards.IndexOf(card);
        if (idx == -1) return;
        ToggleSelectForCut(idx);
    }

    // Inspect whether an instance is currently selected
    public bool IsInstanceSelected(CardInstance card)
    {
        if (card == null || playerHand == null) return false;
        int idx = playerHand.cards.IndexOf(card);
        if (idx == -1) return false;
        return idx == selectedA || idx == selectedB;
    }

    private int GetCutsPerRound()
    {
        // Später: baseCutsPerRound + talismanBonusCuts
        return Mathf.Max(0, baseCutsPerRound);
    }

    public void ClearSelectionPublic()
    {
        ClearSelection();
    }

    private void ClearSelection()
    {
        selectedA = -1;
        selectedB = -1;
    }


    private void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerEntity>();
        if (playerDeck == null) playerDeck = FindFirstObjectByType<PlayerDeck>();
        if (playerHand == null) playerHand = FindFirstObjectByType<PlayerHand>();
        if (handView == null) handView = FindFirstObjectByType<HandView>();
        if (handInteraction == null) handInteraction = FindFirstObjectByType<HandInteractionController>();
        if (targetVignetteUI == null) targetVignetteUI.SetActive(false);
    }

    private void Start()
    {
        // BuildEncounter(firstEncounter);
        //
        // player.ResetForCombat();
        //
        // // 1) Events zuerst verbinden
        // // playerHand.OnCardAdded += handView.AddCard;
        // // playerHand.OnCardRemoved += handView.RemoveCard;
        // playerHand.OnHandChanged += () => handView.Rebuild(playerHand.cards);
        // handView.Rebuild(playerHand.cards); // initial sync
        //
        //
        // // 2) Deck bauen
        // playerDeck.BuildStartingDeck();
        //
        // // 3) Start-Hand ziehen (feuert Events -> Views spawnen)
        // DrawStartingHand();
        //
        // // 4) Combat starten
        // state = CombatState.EnemyChooseIntent;
        // //Debug.Log("Combat started.");
        //
        // AdvanceState();
    }
    
    public void StartEncounter(EncounterDefinition encounter, RunManager run)
    {
        // 0) Cleanup vom vorherigen Fight
        waitingForTarget = false;
        pending = default;
        ExitTargetSelectMode();

        playerHand.Clear();          // brauchst du evtl. implementieren (cards.Clear + OnHandChanged)
        playerDeck.ClearAll();       // Draw/Discard clearen (implementieren falls nicht vorhanden)

        // 1) Player Werte aus Run übernehmen
        player.ResetForCombat();
        player.hp = run.currentHp;
        player.maxHP = run.maxHp;    // falls du maxHP im Run speicherst

        // 2) Deck aus Run übernehmen (WICHTIG: gleiche CardInstances!)
        playerDeck.SetCards(run.deck);  // diese Methode hast du in Schritt 5 schon angepasst

        // 3) Encounter spawnen
        BuildEncounter(encounter);

        // 4) Hand Events verbinden (wie du es schon hast)
        playerHand.OnHandChanged += () => handView.Rebuild(playerHand.cards);
        handView.Rebuild(playerHand.cards);

        // 5) Start-Hand ziehen
        DrawStartingHand();
        
        // 6) UI aktualisieren
        player.RefreshUI();

        // 7) State starten
        state = CombatState.EnemyChooseIntent;
        AdvanceState();
    }

    

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.sKey.wasPressedThisFrame)
        {
            // Debug.Log($"Player-HP: {player.hp}/{player.maxHP} | Player-Armor: {player.armor}");
            Debug.Log($"Current State: {state}");
            
        }

        if (kb.aKey.wasPressedThisFrame)
        {
            foreach (var enemy in activeEnemies)
            {
                if (enemy == null) continue;
                Debug.Log($"Enemy: {enemy.def.name} | HP: {enemy.hp}/{enemy.def.maxHP} | Armor: {enemy.armor} | Marked: {enemy.marked} | Burning: {enemy.burning} | Weakened: {enemy.weakened}");
            }
        }
    }

    public void BuildEncounter(EncounterDefinition encounter)
    {
        activeEnemies.Clear();

        if (encounter == null)
        {
            Debug.LogError("No encounter provided!");
            return;
        }

        foreach (var def in encounter.enemies)
        {
            if (def == null) continue;
            activeEnemies.Add(new EnemyInstance(def));
        }

        if (enemyFieldView == null) enemyFieldView = FindFirstObjectByType<EnemyFieldView>();
        enemyFieldView.Rebuild(activeEnemies);
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
        // Intents wählen
        foreach (var enemy in activeEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            enemy.OnTurnStart();
            enemy.ChooseIntent();
        }
        enemyFieldView.Rebuild(activeEnemies);

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
    }


    private void HandleEnemyExecuteIntent()
    {
        // Intent ausführen
        var ctx = new CombatContext(player, playerDeck, playerHand, activeEnemies, rng);

        foreach (var enemy in activeEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            EnemyMoveResolver.ExecuteMove(enemy, enemy.plannedMove, ctx);
        }
        
        state = CombatState.EndRound;
        AdvanceState();
    }

    private void HandleEndRound()
    {
        Debug.Log("End of round checks");

        if (player.IsDead())
        {
            //Debug.Log("PLAYER DIED – GAME OVER");
            GameFlowController.Instance.OnPlayerDied();
            return;
        }


        //prüfen ob alle Gegner tot sind
        if (AllEnemiesDead())
        {
            Debug.Log("COMBAT WON");

            // HP in RunState zurückschreiben
            RunManager.Instance.currentHp = player.hp;
            player.RefreshUI();
            GameFlowController.Instance.OnCombatWon();
            return;
        }

        state = CombatState.EnemyChooseIntent;
        AdvanceState();
    }
    
    private bool AllEnemiesDead()
    {
        for (int i = 0; i < activeEnemies.Count; i++)
            if (activeEnemies[i] != null && !activeEnemies[i].IsDead)
                return false;
        return true;
    }
    

    public void TryPlayCard(CardInstance card)
    {
        if (state != CombatState.PlayerPlayCard) return;
        if (waitingForTarget) return;                // wichtig: keine zweite Karte während Targeting
        if (card == null) return;
        if (!playerHand.cards.Contains(card)) return;

        // Karte aus Hand raus
        playerHand.Remove(card);
        handView.Rebuild(playerHand.cards);

        var ctx = new CombatContext(player, playerDeck, playerHand, activeEnemies, rng);

        var res = CardResolver.ResolveFrom(card, ctx, startIndex: 0, chosenTarget: null);

        if (res.needsTarget)
        {
            waitingForTarget = true;
            pending = new PendingCardPlay
            {
                card = card,
                nextModuleIndex = res.nextModuleIndex,  // 0 oder 1
                hasChosenTarget = false,
                chosenTarget = null
            };

            EnterTargetSelectMode();
            enemyFieldView.RefreshAll();
            return;
        }

        // fertig ohne Target
        FinishCardPlay(card);
    }
    
    private void FinishCardPlay(CardInstance card)
    {
        playerDeck.Discard(card);

        handView.Rebuild(playerHand.cards);
        enemyFieldView.RefreshAll();

        state = CombatState.EnemyExecuteIntent;
        AdvanceState();
    }



    private void EnterTargetSelectMode()
    {
        //activate Vignette for Target Selection
        if(targetVignetteUI!=null) targetVignetteUI.SetActive(true);
        
        //Karten-Interaktion deaktivieren, Enemy-Interaktion aktivieren
        if (handInteraction != null)
            handInteraction.SetCardsLocked(true);
    }

    private void ExitTargetSelectMode()
    {
        //deactivate Vignette for Target Selection
        if(targetVignetteUI!=null) targetVignetteUI.SetActive(false);
        
        //Zurück in PlayMode Interaction
        if (handInteraction != null)
            handInteraction.SetCardsLocked(false);
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
            return;
        }

        if (selectedB == index)
        {
            selectedB = -1;
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
    }


    public void ExecuteCutHorizontal(CardInstance a, CardInstance b)
    {
        if (cutsRemainingThisRound <= 0) return;
        CardCutter.CutHorizontal(a, b);
        cutsRemainingThisRound--;
    }

    public void ExecuteCutVertical(CardInstance a, CardInstance b)
    {
        if (cutsRemainingThisRound <= 0) return;
        CardCutter.CutVertical(a, b);
        cutsRemainingThisRound--;
    }

    public void OnEnemyClicked(EnemyInstance enemy)
    {
        if (!waitingForTarget) return;
        if (enemy == null || enemy.IsDead || !enemy.IsTargetable) return;

        pending.hasChosenTarget = true;
        pending.chosenTarget = enemy;

        var ctx = new CombatContext(player, playerDeck, playerHand, activeEnemies, rng);

        // ab dem Modul weiter, das beim ersten Durchlauf Target brauchte
        var res = CardResolver.ResolveFrom(
            pending.card,
            ctx,
            startIndex: pending.nextModuleIndex,
            chosenTarget: pending.chosenTarget
        );
        
        if (res.needsTarget)
        {
            Debug.LogError("Still needs target even though chosenTarget is set. Check ResolveModule logic.");
            return;
        }

        // Karte finalisieren
        FinishCardPlay(pending.card);

        // cleanup
        waitingForTarget = false;
        pending = default;
        ExitTargetSelectMode();

        enemyFieldView.RefreshAll();
    }

}