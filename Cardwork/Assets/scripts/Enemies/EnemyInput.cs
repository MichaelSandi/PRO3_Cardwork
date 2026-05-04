using UnityEngine;

public class EnemyInput : MonoBehaviour
{
    private CombatManager combat;
    private EnemyView view;

    private void Awake()
    {
        combat = FindFirstObjectByType<CombatManager>();
        view = GetComponentInParent<EnemyView>();
    }

    public void OnClicked()
    {
        if (combat == null || view == null) return;
        combat.OnEnemyClicked(view.BoundInstance);
    }
    
    public void SetHovered(bool hovered)
    {
        if (view != null)
            view.SetHovered(hovered);
    }

}