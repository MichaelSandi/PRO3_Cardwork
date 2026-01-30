using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public List<EnemyMove> moves = new List<EnemyMove>();

    private EnemyMove currentIntent;

    public EnemyMove ChooseIntent()
    {
        currentIntent = WeightedRandom(moves);
        return currentIntent;
    }

    public void ExecuteIntent(PlayerEntity player)
    {
        if (currentIntent == null)
        {
            Debug.LogWarning("Enemy tried to execute intent, but none was chosen.");
            return;
        }

        Debug.Log($"Enemy uses {currentIntent.moveName} for {currentIntent.damage} damage!");
        player.TakeDamage(currentIntent.damage);
    }

    private EnemyMove WeightedRandom(List<EnemyMove> list)
    {
        if (list == null || list.Count == 0) return null;

        int total = 0;
        foreach (var m in list) total += Mathf.Max(0, m.weight);

        int roll = Random.Range(0, total);
        int sum = 0;

        foreach (var m in list)
        {
            sum += Mathf.Max(0, m.weight);
            if (roll < sum) return m;
        }

        return list[0];
    }
}