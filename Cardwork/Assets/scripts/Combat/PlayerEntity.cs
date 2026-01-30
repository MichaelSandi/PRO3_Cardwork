using UnityEngine;

public class PlayerEntity : MonoBehaviour
{
    public int maxHP = 30;
    public int hp = 30;
    public int armor = 0;

    public void ResetForCombat()
    {
        hp = maxHP;
        armor = 0;
    }

    public void GainArmor(int amount)
    {
        armor += Mathf.Max(0, amount);
        Debug.Log($"Player gains {amount} armor (armor now {armor})");
    }

    public void TakeDamage(int amount)
    {
        amount = Mathf.Max(0, amount);

        int blocked = Mathf.Min(armor, amount);
        armor -= blocked;

        int remaining = amount - blocked;
        hp -= remaining;

        Debug.Log($"Player takes {amount} damage (blocked {blocked}). HP={hp}, Armor={armor}");
    }

    public bool IsDead() => hp <= 0;
}