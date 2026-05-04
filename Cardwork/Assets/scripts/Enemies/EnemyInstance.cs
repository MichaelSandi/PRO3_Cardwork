using TMPro;
using UnityEngine;
using Random = System.Random;

public class EnemyInstance
{
    public EnemyDefinition def;

    public int hp;
    public int maximumHP;
    public int armor;

    public int marked;
    public int burning;
    public int weakened;
    
    public int cloaked; // turns remaining

    public int outgoingDamageBonus;

    public bool IsTargetable => !IsDead && cloaked <= 0;


    public EnemyMove plannedMove;

    public int GetHP()
    {
        if (hp<=0)
            return 0;
        return hp;
    }

    public void ApplyMarked(int x)   { if (x > 0) marked += x; }
    public void ApplyBurning(int x)
    {
        Debug.Log($"ApplyBurning {x} to {def.enemyName}: before={burning}, weakened={weakened}, marked={marked}");
        if (x > 0) burning += x;
        Debug.Log($"AfterBurning: burning={burning}, weakened={weakened}, marked={marked}");
    }

    public void ApplyWeakened(int x)
    {
        Debug.Log($"ApplyWeakened {x} to {def.enemyName}: before={burning}, weakened={weakened}, marked={marked}");
        if (x > 0) weakened += x;
        Debug.Log($"AfterWeakened: burning={burning}, weakened={weakened}, marked={marked}");
        Debug.Log($"EnemyInstance hash: {this.GetHashCode()}");

    }

    

    public EnemyInstance(EnemyDefinition def)
    {
        this.def = def;
        int min = Mathf.Max(1, def.minHP);
        int max = Mathf.Max(min, def.maxHP);

        // inclusive max -> Unity Random.Range int ist max-exklusiv
        hp = UnityEngine.Random.Range(min, max + 1);
        maximumHP = hp;
        armor = 0;
    }

    public bool IsDead => hp <= 0;
    
    public void ChooseIntent()
    {
        plannedMove = WeightedPick(def.moves);

    }
    
    private EnemyMove WeightedPick(System.Collections.Generic.List<EnemyMove> moves)
    {
        if (moves == null || moves.Count == 0) return null;

        int totalWeight = 0;
        foreach (var m in moves)
            totalWeight += m.weight;
        Random rng = new Random();
        int roll = rng.Next(0, totalWeight);
        int cumulative = 0;

        foreach (var m in moves)
        {
            cumulative += m.weight;
            if (roll < cumulative)
                return m;
        }

        return moves[moves.Count - 1];
    }

    
    public void OnTurnStart()
    {
        if (burning > 0)
        {
            TakeDamage(1, ignoreArmor: true);
            burning -= 1;
            if (burning < 0) burning = 0;
        }
        if (cloaked > 0) cloaked--;
    }
    
    public int PeekOutgoingDamage(int baseDamage)
    {
        int dmg = baseDamage;

        if (weakened > 0)
        {
            dmg -= weakened;
            if (dmg < 0) dmg = 0;
        }

        return dmg;
    }

    public int ConsumeWeakenedOnAttack(int baseDamage)
    {
        int dmg = PeekOutgoingDamage(baseDamage) + outgoingDamageBonus;
        if(dmg < 0) return 0;
        //erst hier weakened verbrauchen
        if (weakened > 0)
            weakened = 0;
        if(outgoingDamageBonus > 0)
            outgoingDamageBonus = 0;
        return dmg;
    }



    public void TakeDamage(int amount, bool ignoreArmor = false)
    {
        if (amount <= 0) return;

        // Marked: beim nächsten Mal Schaden: +marked, dann löschen
        if (marked > 0)
        {
            amount += marked;
            marked = 0;
        }

        if (!ignoreArmor && armor > 0)
        {
            int blocked = System.Math.Min(armor, amount);
            armor -= blocked;
            amount -= blocked;
        }

        if (amount > 0)
            hp -= amount;
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        if(hp + amount > maximumHP)
            hp = maximumHP;
        else
            hp += amount;
    }

    
}

