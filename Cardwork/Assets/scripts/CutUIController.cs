using UnityEngine;
using UnityEngine.UI;

public class CutUIController : MonoBehaviour
{
    public Button scissorsButton;
    public Button cancelButton;
    public Button confirmButton;

    public HandInteractionController interaction;
    public CombatManager combat;

    private void Awake()
    {
        if (interaction == null) interaction = FindFirstObjectByType<HandInteractionController>();
        if (combat == null) combat = FindFirstObjectByType<CombatManager>();

        scissorsButton.onClick.AddListener(OnScissors);
        cancelButton.onClick.AddListener(OnCancel);
        confirmButton.onClick.AddListener(OnConfirm);
    }

    private void Update()
    {
        // Buttons live updaten
        bool canCut = combat.CutsRemainingThisRound > 0 && combat.State == CombatState.PlayerPlayCard;
        scissorsButton.interactable = canCut && interaction.mode == HandInteractionController.InteractionMode.PlayMode;

        
        //bool isPlay = interaction.mode == HandInteractionController.InteractionMode.PlayMode;
        bool isSelect = interaction.mode == HandInteractionController.InteractionMode.CutSelectMode;
        bool isAxis = interaction.mode == HandInteractionController.InteractionMode.CutAxisMode;

        cancelButton.gameObject.SetActive(isSelect || isAxis);
        confirmButton.gameObject.SetActive(isSelect || isAxis);

        // Cancel (X) darf IMMER abbrechen
        cancelButton.interactable = isSelect || isAxis;

        // Confirm (Hakerl)
        if (isSelect)
        {
            confirmButton.interactable = interaction.SelectedCount == 2;
        }
        else
        {
            // CutAxisMode oder PlayMode
            confirmButton.interactable = false;
        }
    }

    private void OnScissors()
    {
        interaction.EnterCutSelectMode();
    }

    private void OnCancel()
    {
        interaction.EnterPlayMode();
    }

    private void OnConfirm()
    {
        interaction.ConfirmCutSelection();
    }

}
