using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class MouseClickRaycaster : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask clickableMask;
    
    private CutLineInput hoveredLine;
    private HandInteractionController controller;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        controller = FindFirstObjectByType<HandInteractionController>();
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
        // Hover nur relevant im CutAxisMode
        if (controller == null || controller.mode != HandInteractionController.InteractionMode.CutAxisMode)
        {
            SetHoveredLine(null);
            return;
        }

        if (Mouse.current == null || cam == null) return;

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

            // Dann CardInput
            var card = hit.collider.GetComponentInParent<CardInput>();
            if (card != null)
            {
                card.OnClicked();
                return;
            }
        }
    }
}