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
    [SerializeField] private SpriteRenderer tlArtwork;
    [SerializeField] private TMP_Text tlValueText;

    [Header("TopRight")]
    [SerializeField] private SpriteRenderer trColorIcon;
    [SerializeField] private SpriteRenderer trArtwork;
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

        ApplyCorner(bound.topLeft, tlColorIcon, tlArtwork, tlValueText, CornerPosition.TopLeft);
        ApplyCorner(bound.topRight, trColorIcon,trArtwork, trValueText, CornerPosition.TopRight);
        ApplyCorner(bound.bottomLeft, blColorIcon, null, blValueText, CornerPosition.BottomLeft);
        ApplyCorner(bound.bottomRight, brColorIcon, null, brValueText, CornerPosition.BottomRight);


        if (blNamePartText != null) blNamePartText.text = bound.bottomLeft?.namePartText ?? "";
        if (brNamePartText != null) brNamePartText.text = bound.bottomRight?.namePartText ?? "";

        if (blEffectText != null) blEffectText.text = GetEffectLine(bound.bottomLeft);
        if (brEffectText != null) brEffectText.text = GetEffectLine(bound.bottomRight);

        if (debugText != null) debugText.text = bound.GetName();
    }

    private void ApplyCorner(
        CardCorner corner,
        SpriteRenderer icon,
        SpriteRenderer artwork,
        TMP_Text valueText,
        CornerPosition pos)
    {
        if (corner == null)
        {
            if (icon != null) icon.sprite = null;
            if (valueText != null) valueText.text = "";
            if(artwork != null) artwork.sprite = null;
            return;
        }

        if (icon != null)
            icon.sprite = SpriteFor(corner.color, pos);

        if (valueText != null)
            valueText.text = corner.value.ToString();
        
        if(artwork != null)
            artwork.sprite = corner.artwork;
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

        int xVal = bound.GetSum(m.xColor);
        int yVal = bound.GetSum(m.yColor);
        string x = $"{FormatColoredSum(xVal, m.xColor)}";
        string y = $"{FormatColoredSum(yVal, m.yColor)}";


        return m.type switch
        {
            EffectType.RegainLife => $"Heal {x}",
            EffectType.GainArmor => $"Shield {x}",
            EffectType.DrawCards => $"Draw {x}",
            EffectType.DiscardCards => $"Discard {x}",
            EffectType.Marked => $"Mark {x}",
            EffectType.Burning => $"Ignite {x}",
            EffectType.Weakened => $"Weaken {x}",

            EffectType.DealDamage => m.damageDistribution switch
            {
                DamageDistribution.OneEnemy => $"Deal {x}",
                DamageDistribution.AllEnemies => $"Deal {x} to all",
                DamageDistribution.SplitAmongAllEnemies => $"Deal {x} randomly split",
                DamageDistribution.SplitAmongYRandomEnemies => $"Deal {x} randomly among {y} enemies",
                _ => $"Deal {x} damage"
            },

            _ => ""
        };
    }
    
    private string FormatColoredSum(int value, CardColors color)
    {
        string hex = GetHexColor(color);
        return $"<b><color={hex}>{value}</color></b>";
    }

    private string GetHexColor(CardColors color)
    {
        return color switch
        {
            CardColors.Pink   => "#b81256",
            CardColors.Blue   => "#2d2fba",
            CardColors.Orange => "#ad6621",
            CardColors.Yellow => "#a79727",
            CardColors.Silver => "#7e95ab",
            _                 => "#FFFFFF"
        };
    }
    
}
