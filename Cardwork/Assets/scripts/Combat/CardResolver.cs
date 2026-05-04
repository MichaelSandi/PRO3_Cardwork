using System.Collections.Generic;

public static class CardResolver
{
    public static ResolveResult ResolveFrom(CardInstance card, CombatContext ctx, int startIndex, EnemyInstance chosenTarget)
    {
        for (int i = startIndex; i < 2; i++)
        {
            var module = (i == 0) ? card.bottomLeft?.effectModule : card.bottomRight?.effectModule;
            var res = ResolveModule(card, module, ctx, chosenTarget, i);
            if (res.needsTarget) return res;
        }
        return ResolveResult.Done();
    }



    // NEW: Execute with chosen target (wenn TargetSelection passiert)
    private static void ExecuteSingleTarget(EffectModuleData module, int x, int y, CombatContext ctx, EnemyInstance target)
    {
        switch (module.type)
        {
            case EffectType.Marked:   if (x > 0) target.ApplyMarked(x); break;
            case EffectType.Burning:  if (x > 0) target.ApplyBurning(x); break;
            case EffectType.Weakened: if (x > 0) target.ApplyWeakened(x); break;

            case EffectType.DealDamage:
                if (x > 0) target.TakeDamage(x, ignoreArmor: false);
                break;
        }
    }

    private static ResolveResult ResolveModule(CardInstance card, EffectModuleData module, CombatContext ctx, EnemyInstance chosenTarget, int moduleIndex)
    {
        if (module == null || module.type == EffectType.None) return ResolveResult.Done();

        int x = card.GetSum(module.xColor);
        int y = card.GetSum(module.yColor);

        bool needsTarget =
            module.type == EffectType.Marked ||
            module.type == EffectType.Burning ||
            module.type == EffectType.Weakened ||
            (module.type == EffectType.DealDamage && module.damageDistribution == DamageDistribution.OneEnemy);

        if (needsTarget)
        {
            var candidates = ctx.GetAllLivingEnemies(false);
            if (candidates.Count == 0) return ResolveResult.Done();

            if (chosenTarget != null)
            {
                ExecuteSingleTarget(module, x, y, ctx, chosenTarget);
                return ResolveResult.Done();
            }

            if (candidates.Count == 1)
            {
                ExecuteSingleTarget(module, x, y, ctx, candidates[0]);
                return ResolveResult.Done();
            }

            return ResolveResult.Need(moduleIndex);
        }

        ExecuteNonTarget(module, x, y, ctx, card);
        return ResolveResult.Done();
    }
    
    private static void ExecuteNonTarget(EffectModuleData module, int x, int y, CombatContext ctx, CardInstance card)
    {
        switch (module.type)
        {
            case EffectType.RegainLife:
                ctx.player.RegainLife(x);
                break;

            case EffectType.GainArmor:
                ctx.player.GainArmor(x);
                break;

            case EffectType.DrawCards:
                DrawCards(ctx, x);
                break;

            case EffectType.DiscardCards:
                DiscardRandom(ctx, x);
                break;

            case EffectType.DealDamage:
                // Hier sind alle non-target distributions drin (All / Split / etc.)
                ResolveDamage(card, ctx, x, y, module.damageDistribution);
                break;

            // Status sind oben im needsTarget block, kommen hier nicht rein
        }
    }



    private static void ResolveDamage(CardInstance card, CombatContext ctx, int x, int y, DamageDistribution dist)
    {
        if (x <= 0) return;

        switch (dist)
        {
            case DamageDistribution.OneEnemy:
            {
                var target = ctx.GetDefaultTarget();
                if (target != null) target.TakeDamage(x, ignoreArmor: false);
                break;
            }

            case DamageDistribution.AllEnemies:
            {
                var all = ctx.GetAllLivingEnemies(false);
                for (int i = 0; i < all.Count; i++)
                    all[i].TakeDamage(x, ignoreArmor: false);
                break;
            }

            case DamageDistribution.SplitAmongAllEnemies:
            {
                var all = ctx.GetAllLivingEnemies(false);
                RandomSplitDamage(all, x, ctx);
                break;
            }

            case DamageDistribution.SplitAmongYRandomEnemies:
            {
                // Deine Korrektur: Total X wird in 1er Schritten verteilt, aber nur auf Y random "slots"
                // with replacement: Targets können doppelt vorkommen, dadurch gewichtet
                var all = ctx.GetAllLivingEnemies(false);
                if (all.Count == 0) return;

                int slotsCount = System.Math.Max(0, y);
                if (slotsCount == 0) return;

                var slots = new List<EnemyInstance>(slotsCount);
                for (int i = 0; i < slotsCount; i++)
                {
                    int idx = ctx.rng.Next(0, all.Count);
                    slots.Add(all[idx]);
                }

                RandomSplitDamage(slots, x, ctx);
                break;
            }
        }
    }

    private static void RandomSplitDamage(List<EnemyInstance> targets, int totalDamage, CombatContext ctx)
    {
        if (targets == null || targets.Count == 0) return;
        if (totalDamage <= 0) return;

        for (int i = 0; i < totalDamage; i++)
        {
            int idx = ctx.rng.Next(0, targets.Count);
            var t = targets[idx];
            if (t != null && !t.IsDead && t.IsTargetable)
                t.TakeDamage(1, ignoreArmor: false);
        }
    }

    private static void DrawCards(CombatContext ctx, int x)
    {
        if (x <= 0) return;

        for (int i = 0; i < x; i++)
        {
            var card = ctx.deck.DrawOne();
            if (card == null) return;

            bool added = ctx.hand.Add(card);
            if (!added)
            {
                // wenn Hand voll: Karte zurück oben drauf oder in DrawPile lassen
                ctx.deck.PutBackOnTop(card);   
                return;
            }
        }
    }

    private static void DiscardRandom(CombatContext ctx, int x)
    {
        if (x <= 0) return;

        for (int i = 0; i < x; i++)
        {
            if (ctx.hand.cards.Count == 0) return;

            int idx = ctx.rng.Next(0, ctx.hand.cards.Count);
            var c = ctx.hand.cards[idx];

            ctx.hand.Remove(c);
            ctx.deck.Discard(c);
        }
    }
}
