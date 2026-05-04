using System;
using System.Collections.Generic;

public class CombatContext
{
    public System.Random rng;

    public PlayerEntity player;
    public PlayerDeck deck;
    public PlayerHand hand;
    public List<EnemyInstance> enemies;

    public CombatContext(
        PlayerEntity player,
        PlayerDeck deck,
        PlayerHand hand,
        List<EnemyInstance> enemies,
        System.Random rng = null)
    {
        this.player = player;
        this.deck = deck;
        this.hand = hand;
        this.enemies = enemies;
        this.rng = rng ?? new System.Random();
    }

    public EnemyInstance GetDefaultTarget()
    {
        if (enemies == null) return null;

        for (int i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            if (e != null && !e.IsDead && e.IsTargetable)
                return e;
        }

        return null;
    }

    public List<EnemyInstance> GetAllLivingEnemies(bool includeUntargetable)
    {
        var list = new List<EnemyInstance>();
        for (int i = 0; i < enemies.Count; i++)
        {
            var e = enemies[i];
            if (e == null || e.IsDead) continue;
            if (!includeUntargetable && !e.IsTargetable) continue;
            list.Add(e);
        }
        return list;
    }

}