using UnityEngine;

public static class CardResolver
{
    public static void Play(CardInstance card, PlayerEntity player, EnemyEntity enemy)
    {
        int orange = card.GetSum(CardColors.Orange);
        int yellow = card.GetSum(CardColors.Yellow);
        int blue = card.GetSum(CardColors.Blue);
        int pink = card.GetSum(CardColors.Pink);

        if (orange > 0)
        {
            enemy.TakeDamage(orange);
            Debug.Log($"Card effect: Deal {orange} orange damage.");
        }

        if (yellow > 0)
        {
            player.GainArmor(yellow);
            Debug.Log($"Card effect: Gain {yellow} yellow armor.");
        }

        if (blue > 0)
        {
            // Draw ist im CombatManager/Hand gelöst, deshalb geben wir nur zurück (siehe unten)
            Debug.Log($"Card effect: Draw {blue} blue cards.");
        }
        
        if(pink > 0)
        {
            player.TakeDamage(-pink); // Negative damage = Heal
            Debug.Log($"Card effect: Heal {pink} HP.");
        }
    }

    public static int GetBlueDraw(CardInstance card)
        => card.GetSum(CardColors.Blue);
}