using System.Collections.Generic;
using UnityEngine;

public static class EnemyMoveResolver
{
    public static void ExecuteMove(EnemyInstance source, EnemyMove move, CombatContext ctx)
    {
        if (source == null || source.IsDead) return;
        if (move == null) return;

        for (int i = 0; i < move.effects.Count; i++)
        {
            ExecuteEffect(source, move.effects[i], ctx);
        }
    }

    private static void ExecuteEffect(EnemyInstance source, EnemyMoveEffect eff, CombatContext ctx)
    {
        // Note: For buffs/heals/armor we usually include untargetables.
        List<EnemyInstance> living = ctx.GetAllLivingEnemies(includeUntargetable: true);

        switch (eff.type)
        {
            case EnemyEffectType.DealDamagePlayer:
            {
                int dmg = Mathf.Max(0, eff.amount);
                if (dmg <= 0) break;

                // If you use weakened on enemy outgoing, apply here.
                int final = dmg + Mathf.Max(0, source.outgoingDamageBonus);

                // If bonus should be "next attack only", uncomment:
                // source.outgoingDamageBonus = 0;

                ctx.player.TakeDamage(final);
                break;
            }

            case EnemyEffectType.GainArmorSelf:
            {
                int x = Mathf.Max(0, eff.amount);
                source.armor += x;
                break;
            }

            case EnemyEffectType.GainArmorAllEnemies:
            {
                int x = Mathf.Max(0, eff.amount);
                if (x <= 0) break;

                for (int i = 0; i < living.Count; i++)
                    living[i].armor += x;

                break;
            }

            case EnemyEffectType.GainArmorRandomEnemy:
            {
                int x = Mathf.Max(0, eff.amount);
                if (x <= 0) break;
                if (living.Count == 0) break;

                int idx = ctx.rng.Next(0, living.Count);
                living[idx].armor += x;
                break;
            }

            case EnemyEffectType.BuffAlliesDamage:
            {
                int x = Mathf.Max(0, eff.amount);
                if (x <= 0) break;

                for (int i = 0; i < living.Count; i++)
                {
                    // If you do NOT want to buff self, add:
                    // if (living[i] == source) continue;
                    living[i].outgoingDamageBonus += x;
                }
                break;
            }

            case EnemyEffectType.BuffRandomEnemyDamage:
            {
                int x = Mathf.Max(0, eff.amount);
                if (x <= 0) break;
                if (living.Count == 0) break;

                int idx = ctx.rng.Next(0, living.Count);
                living[idx].outgoingDamageBonus += x;
                break;
            }

            case EnemyEffectType.CloakSelf:
            {
                int t = Mathf.Max(0, eff.duration);
                if (t <= 0) break;

                source.cloaked = Mathf.Max(source.cloaked, t);
                break;
            }

            case EnemyEffectType.HealSelf:
            {
                int x = Mathf.Max(0, eff.amount);
                source.Heal(x);
                break;
            }

            case EnemyEffectType.HealRandomEnemy:
            {
                int x = Mathf.Max(0, eff.amount);
                if (x <= 0) break;
                if (living.Count == 0) break;

                int idx = ctx.rng.Next(0, living.Count);
                living[idx].Heal(x);
                break;
            }

            case EnemyEffectType.HealAllEnemies:
            {
                int x = Mathf.Max(0, eff.amount);
                if (x <= 0) break;

                for (int i = 0; i < living.Count; i++)
                    living[i].Heal(x);

                break;
            }

            case EnemyEffectType.CleanseDebuffsSelf:
            {
                // Define what "debuffs" mean for enemies in your game:
                // suggested: burning, marked, weakened
                source.burning = 0;
                source.marked = 0;
                source.weakened = 0;
                break;
            }

            case EnemyEffectType.DiscardPlayerCard:
            {
                int x = Mathf.Max(0, eff.amount);
                if (x <= 0) break;

                ForceDiscard(ctx, x);
                break;
            }

            default:
                Debug.LogWarning($"EnemyMoveResolver: Unhandled effect {eff.type}");
                break;
        }
    }

    private static void ForceDiscard(CombatContext ctx, int count)
    {
        // discard X random cards from player's hand
        for (int i = 0; i < count; i++)
        {
            if (ctx.hand.cards.Count == 0) return;

            int idx = ctx.rng.Next(0, ctx.hand.cards.Count);
            var card = ctx.hand.cards[idx];

            ctx.hand.Remove(card);
            ctx.deck.Discard(card);
        }
    }
}
