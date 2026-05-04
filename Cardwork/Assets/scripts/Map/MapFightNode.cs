using UnityEngine;

public class MapFightNode : MonoBehaviour
{
    [SerializeField] private EncounterDefinition nextEncounter;

    public void OnClicked()
    {
        GameFlowController.Instance.StartCombat(nextEncounter);
    }
}
