using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }

    [Header("Cameras")] [SerializeField] private Camera mainCamera; // Combat
    [SerializeField] private Camera mapCamera;

    [Header("Systems")] [SerializeField] private CombatManager combatManager;
    [SerializeField] private MapController mapController;
    [SerializeField] private GameObject combatUI;
    [SerializeField] private GameObject mapUI;
    [SerializeField] private MapLayoutDefinition mapLayout;

    public bool IsInMapMode { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        EnterMapMode();
    }

    // ===== MODES =====

    public void EnterMapMode()
    {
        IsInMapMode = true;

        mainCamera.gameObject.SetActive(false);
        mapCamera.gameObject.SetActive(true);

        combatUI.SetActive(false);
        mapUI.SetActive(true);

        combatManager.enabled = false;
        mapController.gameObject.SetActive(true);
    }

    public void StartCombat(EncounterDefinition encounter)
    {
        IsInMapMode = false;

        mapController.gameObject.SetActive(false);

        mapCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);

        mapUI.SetActive(false);
        combatUI.SetActive(true);

        combatManager.enabled = true;
        combatManager.StartEncounter(encounter, RunManager.Instance);
    }

    // ===== CALLBACKS =====

    public void OnCombatWon()
    {
        // mark current node cleared + unlock neighbors
        var run = RunManager.Instance;
        string nodeId = RunManager.Instance.currentNodeId;
        if (run != null && run.mapState != null)
        {
            if (!string.IsNullOrEmpty(nodeId))
            {
                MapGenerator.MarkClearedAndUnlockNext(
                    mapLayout, // ← Referenz hinzufügen!
                    run.mapState,
                    nodeId
                );
            }
        }

        var node = mapLayout.GetNode(nodeId);
        if (node != null && node.isEnd)
        {
            OnRunWon();
            return;
        }

        EnterMapMode();
    }
    
    private void OnRunWon()
    {
        SceneManager.LoadScene("VictoryScene");
    }



    public void OnPlayerDied()
    {
        RunManager.Instance.ResetRun();
        UnityEngine.SceneManagement.SceneManager.LoadScene("DefeatScene");
    }
}