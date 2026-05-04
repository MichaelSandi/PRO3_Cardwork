using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MapRaycaster : MonoBehaviour
{
    [SerializeField] private Camera mapCamera;
    [SerializeField] private LayerMask mapClickableMask;

    private MapNodeInput hovered;

    private void Awake()
    {
        if (mapCamera == null) mapCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        if (GameFlowController.Instance != null && !GameFlowController.Instance.IsInMapMode)
        {
            SetHovered(null);
            return;
        }

        if (Mouse.current == null || mapCamera == null) return;

        // UI block if you have any overlay UI in map mode
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            SetHovered(null);
            return;
        }

        UpdateHover();

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryClick();
    }

    private void UpdateHover()
    {
        Ray ray = mapCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, mapClickableMask))
        {
            var input = hit.collider.GetComponentInParent<MapNodeInput>();
            SetHovered(input);
        }
        else SetHovered(null);
    }

    private void TryClick()
    {
        Ray ray = mapCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, mapClickableMask))
        {
            var input = hit.collider.GetComponentInParent<MapNodeInput>();
            if (input != null) input.OnClicked();
        }
    }

    private void SetHovered(MapNodeInput next)
    {
        if (hovered == next) return;
        if (hovered != null) hovered.SetHovered(false);
        hovered = next;
        if (hovered != null) hovered.SetHovered(true);
    }
}