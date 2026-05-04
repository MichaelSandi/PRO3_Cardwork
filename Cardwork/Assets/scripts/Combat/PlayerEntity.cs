using TMPro;
using UnityEngine;

public class PlayerEntity : MonoBehaviour
{
    public int maxHP = 30;
    public int hp = 30;
    public int armor = 0;
    
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI armorText;

    public void ResetForCombat()
    {
        hp = maxHP;
        armor = 0;
        RefreshUI();
    }

    public void GainArmor(int amount)
    {
        armor += Mathf.Max(0, amount);
        Debug.Log($"Player gains {amount} armor (armor now {armor})");
        RefreshUI();
    }

    public void TakeDamage(int amount)
    {
        amount = Mathf.Max(0, amount);

        int blocked = Mathf.Min(armor, amount);
        armor -= blocked;

        int remaining = amount - blocked;
        hp -= remaining;

        Debug.Log($"Player takes {amount} damage (blocked {blocked}). HP={hp}, Armor={armor}");
        RefreshUI();
    }
    
    public void RegainLife(int amount)
    {
        amount = Mathf.Max(0, amount);
        hp = Mathf.Min(maxHP, hp + amount);
        Debug.Log($"Player heals {amount}. HP={hp}/{maxHP}");
        RefreshUI();
    }


    public bool IsDead() => hp <= 0;

    public void RefreshUI()
    {
        healthText.text = $"HP: {hp}/{maxHP}";
        if(armor > 0)
            armorText.text = $"Armor: {armor}";
        else
            armorText.text = string.Empty;
        
    }
}