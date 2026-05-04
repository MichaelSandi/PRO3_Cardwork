using TMPro;
using UnityEngine;

public class EnemyView : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Status Badges")]
    [SerializeField] private GameObject markedBadge;
    [SerializeField] private TMP_Text markedText;

    [SerializeField] private GameObject burningBadge;
    [SerializeField] private TMP_Text burningText;

    [SerializeField] private GameObject weakenedBadge;
    [SerializeField] private TMP_Text weakenedText;
    
    [SerializeField] private GameObject hoverMarker;
    
    [SerializeField] private TextMeshPro intendedMoveText;

    public EnemyInstance BoundInstance { get; private set; }

    public void Bind(EnemyInstance inst)
    {
        BoundInstance = inst;
        Refresh();
    }

    public void Refresh()
    {
        if (BoundInstance == null || BoundInstance.def == null) return;

        if (spriteRenderer != null)
            spriteRenderer.sprite = BoundInstance.def.sprite;

        string armorText = BoundInstance.armor > 0 ? $"+ {BoundInstance.armor}" : "";
        if (healthText != null)
            healthText.text = $"{BoundInstance.GetHP()}/{BoundInstance.maximumHP} {armorText}";

        ApplyBadge(markedBadge, markedText, BoundInstance.marked);
        ApplyBadge(burningBadge, burningText, BoundInstance.burning);
        ApplyBadge(weakenedBadge, weakenedText, BoundInstance.weakened);
        
        if (intendedMoveText != null)
        {
            if (BoundInstance.plannedMove != null)
                intendedMoveText.text = BoundInstance.plannedMove.moveName;
            else
                intendedMoveText.text = "";
        }

        if (BoundInstance.GetHP() == 0)
        {
            spriteRenderer.color = new Color(0f, 0f, 0f, 0.4f); // halbtransparent
            intendedMoveText.text = "DEFEATED";
            markedBadge.SetActive(false);
            burningBadge.SetActive(false);
            weakenedBadge.SetActive(false);
        }
    }

    private void ApplyBadge(GameObject badge, TMP_Text txt, int value)
    {
        if (badge != null) badge.SetActive(value > 0);
        if (txt != null) txt.text = value > 0 ? value.ToString() : "";
    }

    public void SetHovered(bool hovered)
    {
        if(hoverMarker != null)
            hoverMarker.SetActive(hovered);
    }
}