using UnityEngine;

public class EnemyEntity : MonoBehaviour
{
    public int maxHP = 40;
    public int hp = 40;

    public void ResetForCombat()
    {
        hp = maxHP;
    }

    public void TakeDamage(int amount)
    {
        amount = Mathf.Max(0, amount);
        hp -= amount;
        Debug.Log($"Enemy takes {amount} damage. HP={hp}");
    }

    public bool IsDead() => hp <= 0;
}