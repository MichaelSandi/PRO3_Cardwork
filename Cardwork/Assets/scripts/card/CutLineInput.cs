using UnityEngine;

public class CutLineInput : MonoBehaviour
{
    public enum Axis
    {
        Horizontal,
        Vertical
    }

    [SerializeField] private Axis axis;
    public Axis GetAxis() => axis;
    private HandInteractionController controller;
    private CardView cardView;

    [Header("Hover Visual")] [SerializeField]
    private SpriteRenderer lineRenderer;

    [SerializeField] private float hoverScale = 1.1f;

    private Vector3 baseScale;

    private void Awake()
    {
        controller = FindFirstObjectByType<HandInteractionController>();
        cardView = GetComponentInParent<CardView>();
        if (lineRenderer == null) lineRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }
    
    public void SetHovered(bool hovered)
    {
        if (lineRenderer == null) return;

        // Simple MVP hover: scale up a bit
        transform.localScale = hovered ? baseScale * hoverScale : baseScale;

        // Optional: sorting boost (falls Linie sonst untergeht)
        // lineRenderer.sortingOrder = hovered ? 100 : 50;
    }

    public void OnClicked()
    {
        if (controller == null || cardView == null) return;
        controller.OnCutAxisClicked(cardView.BoundInstance, axis);
    }
}