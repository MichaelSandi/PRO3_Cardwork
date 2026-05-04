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

        if (scissorsButton != null) scissorsButton.onClick.AddListener(OnScissors);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
    }

    private void OnDestroy()
    {
        if (scissorsButton != null) scissorsButton.onClick.RemoveListener(OnScissors);
        if (cancelButton != null) cancelButton.onClick.RemoveListener(OnCancel);
        if (confirmButton != null) confirmButton.onClick.RemoveListener(OnConfirm);
    }

    private void Update()
    {
        if (combat == null || interaction == null) return;

        bool canCut = combat.CutsRemainingThisRound > 0 && combat.State == CombatState.PlayerPlayCard;
        if (scissorsButton != null)
            scissorsButton.interactable = canCut && interaction.mode == HandInteractionController.InteractionMode.PlayMode;

        bool isSelect = interaction.mode == HandInteractionController.InteractionMode.CutSelectMode;
        bool isAxis = interaction.mode == HandInteractionController.InteractionMode.CutAxisMode;

        if (cancelButton != null) cancelButton.gameObject.SetActive(isSelect || isAxis);
        if (confirmButton != null) confirmButton.gameObject.SetActive(isSelect || isAxis);

        if (cancelButton != null) cancelButton.interactable = isSelect || isAxis;

        if (isSelect)
        {
            if (confirmButton != null) confirmButton.interactable = interaction.SelectedCount == 2;
        }
        else
        {
            if (confirmButton != null) confirmButton.interactable = false;
        }
    }

    private void OnScissors()
    {
        interaction?.EnterCutSelectMode();
    }

    private void OnCancel()
    {
        interaction?.EnterPlayMode();
    }

    private void OnConfirm()
    {
        interaction?.ConfirmCutSelection();
    }

}
