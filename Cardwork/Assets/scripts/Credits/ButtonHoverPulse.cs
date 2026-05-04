using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverPulse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scale Pulse")]
    [SerializeField] private float pulseAmount = 0.06f;     // z.B. 6% größer/kleiner
    [SerializeField] private float pulseSpeed = 6f;         // höher = schneller

    [Header("Target Graphic (optional)")]
    [SerializeField] private Graphic targetGraphic;         // Image/Text, das gefärbt werden soll

    private RectTransform rt;
    private Vector3 baseScale;
    private Color baseColor;
    private bool hovering;
    private float t;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        baseScale = rt.localScale;

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>(); // Button Image am selben GO

        if (targetGraphic != null)
            baseColor = targetGraphic.color;
    }

    private void OnDisable()
    {
        // Reset, falls Objekt deaktiviert wird während Hover
        ResetVisuals();
        hovering = false;
    }

    private void Update()
    {
        if (!hovering) return;

        t += Time.unscaledDeltaTime; // läuft auch wenn Time.timeScale=0 im Menü

        // Puls: sin zwischen -1 und +1
        float s = 1f + Mathf.Sin(t * pulseSpeed) * pulseAmount;
        rt.localScale = baseScale * s;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        t = 0f; // optional: jedes Hover neu starten
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        ResetVisuals();
    }

    private void ResetVisuals()
    {
        if (rt != null) rt.localScale = baseScale;
        if (targetGraphic != null) targetGraphic.color = baseColor;
    }
}
