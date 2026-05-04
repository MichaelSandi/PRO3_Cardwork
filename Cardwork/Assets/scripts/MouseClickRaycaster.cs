using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class MouseClickRaycaster : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask clickableMask;

    private CutLineInput hoveredLine;
    private HandInteractionController controller;

    private EnemyInput hoveredEnemy;
    private CombatManager combat;


    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        controller = FindFirstObjectByType<HandInteractionController>();
        combat = FindFirstObjectByType<CombatManager>();
    }

    private void Update()
    {
        // UI blockt Worldspace
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            SetHoveredLine(null);
            return;
        }

        UpdateHover();
        HandleClick();
    }

    private void UpdateHover()
    {
        if (Mouse.current == null || cam == null) return;

        // UI blockt Worldspace
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            SetHoveredLine(null);
            SetHoveredEnemy(null);
            return;
        }

        // Priorität 1: CutAxisMode (Linien hover)
        if (controller != null && controller.mode == HandInteractionController.InteractionMode.CutAxisMode)
        {
            SetHoveredEnemy(null); // keine enemy hover gleichzeitig
            HoverCutLines();
            return;
        }

        // Priorität 2: TargetSelectMode (Enemy hover)
        if (combat != null && combat.IsWaitingForTarget)
        {
            SetHoveredLine(null);
            HoverEnemies();
            return;
        }

        // sonst: alles aus
        SetHoveredLine(null);
        SetHoveredEnemy(null);
    }

    private void HoverCutLines()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, clickableMask))
        {
            var line = hit.collider.GetComponentInParent<CutLineInput>();
            SetHoveredLine(line);
        }
        else
        {
            SetHoveredLine(null);
        }
    }

    private void HoverEnemies()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, clickableMask))
        {
            var enemy = hit.collider.GetComponentInParent<EnemyInput>();
            SetHoveredEnemy(enemy);
        }
        else
        {
            SetHoveredEnemy(null);
        }
    }

    private void SetHoveredEnemy(EnemyInput newEnemy)
    {
        if (hoveredEnemy == newEnemy) return;

        if (hoveredEnemy != null) hoveredEnemy.SetHovered(false);
        hoveredEnemy = newEnemy;
        if (hoveredEnemy != null) hoveredEnemy.SetHovered(true);
    }


    private void SetHoveredLine(CutLineInput newLine)
    {
        if (hoveredLine == newLine) return;

        if (hoveredLine != null) hoveredLine.SetHovered(false);
        hoveredLine = newLine;
        if (hoveredLine != null) hoveredLine.SetHovered(true);
    }

    private void HandleClick()
    {
        if (Mouse.current == null || cam == null) return;

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, clickableMask))
        {
            // Erst CutLine
            var line = hit.collider.GetComponentInParent<CutLineInput>();
            if (line != null)
            {
                line.OnClicked();
                return;
            }

            //Danach EnemyInput
            var enemy = hit.collider.GetComponentInParent<EnemyInput>();
            if (enemy != null)
            {
                enemy.OnClicked();
                return;
            }


            // Dann CardInput
            if (combat == null || !combat.IsWaitingForTarget)
            {
                var card = hit.collider.GetComponentInParent<CardInput>();
                if (card != null)
                {
                    card.OnClicked();
                    return;
                }
            }
        }
    }
}