using UnityEngine;
using TMPro;

public enum CornerPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public class CardView : MonoBehaviour
{
    [Header("Face Roots")]
    [SerializeField] private GameObject frontRoot;
    [SerializeField] private GameObject backRoot;

    [Header("TopLeft")]
    [SerializeField] private SpriteRenderer tlColorIcon;
    [SerializeField] private TMP_Text tlValueText;

    [Header("TopRight")]
    [SerializeField] private SpriteRenderer trColorIcon;
    [SerializeField] private TMP_Text trValueText;

    [Header("BottomLeft")]
    [SerializeField] private SpriteRenderer blColorIcon;
    [SerializeField] private TMP_Text blValueText;
    [SerializeField] private TMP_Text blNamePartText;
    [SerializeField] private TMP_Text blEffectText;

    [Header("BottomRight")]
    [SerializeField] private SpriteRenderer brColorIcon;
    [SerializeField] private TMP_Text brValueText;
    [SerializeField] private TMP_Text brNamePartText;
    [SerializeField] private TMP_Text brEffectText;

    [Header("Optional global text")]
    [SerializeField] private TMP_Text debugText;

    [Header("Color SpriteLibrary")]
    [SerializeField] private CornerColorSpriteLibrary spriteLibrary;
    
    private CardInstance bound;
    public CardInstance BoundInstance => bound;
    
    [SerializeField] private GameObject selectOutline;
    
    [Header("Cut Lines")]
    [SerializeField] private GameObject cutLineH;
    [SerializeField] private GameObject cutLineV;

    public void SetCutLinesVisible(bool visible)
    {
        if (cutLineH != null) cutLineH.SetActive(visible);
        if (cutLineV != null) cutLineV.SetActive(visible);
    }

    

    public void SetSelected(bool selected)
    {
        if (selectOutline != null)
            selectOutline.SetActive(selected);
    }

    public void SetFaceUp(bool faceUp)
    {
        if (frontRoot != null) frontRoot.SetActive(faceUp);
        if (backRoot != null) backRoot.SetActive(!faceUp);
    }

    public void Bind(CardInstance instance)
    {
        if (bound != null)
            bound.OnChanged -= Refresh; // wichtig: unsubscribe

        bound = instance;

        if (bound != null)
            bound.OnChanged += Refresh;

        Refresh();
    }

    public void Refresh()
    {
        if (bound == null) return;

        ApplyCorner(bound.topLeft, tlColorIcon, tlValueText, CornerPosition.TopLeft);
        ApplyCorner(bound.topRight, trColorIcon, trValueText, CornerPosition.TopRight);
        ApplyCorner(bound.bottomLeft, blColorIcon, blValueText, CornerPosition.BottomLeft);
        ApplyCorner(bound.bottomRight, brColorIcon, brValueText, CornerPosition.BottomRight);


        if (blNamePartText != null) blNamePartText.text = bound.bottomLeft?.namePartText ?? "";
        if (brNamePartText != null) brNamePartText.text = bound.bottomRight?.namePartText ?? "";

        if (blEffectText != null) blEffectText.text = GetEffectLine(bound.bottomLeft);
        if (brEffectText != null) brEffectText.text = GetEffectLine(bound.bottomRight);

        if (debugText != null) debugText.text = bound.GetName();
    }

    private void ApplyCorner(
        CardCorner corner,
        SpriteRenderer icon,
        TMP_Text valueText,
        CornerPosition pos)
    {
        if (corner == null)
        {
            if (icon != null) icon.sprite = null;
            if (valueText != null) valueText.text = "";
            return;
        }

        if (icon != null)
            icon.sprite = SpriteFor(corner.color, pos);

        if (valueText != null)
            valueText.text = corner.value.ToString();
    }


    private Sprite SpriteFor(CardColors color, CornerPosition pos)
    {
        if (spriteLibrary == null)
        {
            Debug.LogError("CardView has no SpriteLibrary assigned!");
            return null;
        }

        return spriteLibrary.GetSprite(color, pos);
    }


    // MVP: generiert einen lesbaren Text aus EffectModuleData + X/Y Farben
    private string GetEffectLine(CardCorner corner)
    {
        var m = corner?.effectModule;
        if (m == null || m.type == EffectType.None) return "";

        // Optional override
        if (!string.IsNullOrWhiteSpace(m.effectTextOverride))
            return m.effectTextOverride;

        string x = $"[sum {m.xColor}]";
        string y = $"[sum {m.yColor}]";

        return m.type switch
        {
            EffectType.RegainLife => $"Regain life {x}",
            EffectType.GainArmor => $"You gain {x} Armor",
            EffectType.DrawCards => $"Draw {x} cards",
            EffectType.DiscardCards => $"Discard {x} cards",
            EffectType.Marked => $"Apply Marked {x}",
            EffectType.Burning => $"Apply Burning {x}",
            EffectType.Weakened => $"Apply Weakened {x}",

            EffectType.DealDamage => m.damageDistribution switch
            {
                DamageDistribution.OneEnemy => $"Deal {x} damage to one enemy",
                DamageDistribution.AllEnemies => $"Deal {x} damage to all enemies",
                DamageDistribution.SplitAmongAllEnemies => $"Deal {x} damage randomly split among all enemies",
                DamageDistribution.SplitAmongYRandomEnemies => $"Deal {x} damage randomly split among {y} random enemies",
                _ => $"Deal {x} damage"
            },

            _ => ""
        };
    }
}
