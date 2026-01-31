using UnityEngine;

/// <summary>
/// Static class containing card effect implementations.
/// Each effect corresponds to a card type action.
/// </summary>
public static class Effects
{
    /// <summary>
    /// Applies attack damage to the monster.
    /// </summary>
    /// <param name="damage">Amount of damage to deal.</param>
    /// <param name="monster">Target monster.</param>
    public static void ApplyAttack(int damage, Monster monster)
    {
        if (monster != null)
        {
            monster.TakeDamage(damage);
            Debug.Log($"Attack effect: Dealt {damage} damage to monster.");
        }
    }

    /// <summary>
    /// Applies defense to the player.
    /// </summary>
    /// <param name="amount">Amount of defense to add.</param>
    /// <param name="player">Target player.</param>
    public static void ApplyDefense(int amount, Player player)
    {
        if (player != null)
        {
            player.AddDefense(amount);
            Debug.Log($"Defense effect: Added {amount} defense to player.");
        }
    }

    /// <summary>
    /// Applies bonus effect (bonus damage based on monster stage).
    /// </summary>
    /// <param name="basePoints">Base bonus points.</param>
    /// <param name="monster">Target monster for bonus calculation.</param>
    public static void ApplyBonus(int basePoints, Monster monster)
    {
        if (monster != null)
        {
            // Bonus cards deal extra damage based on monster's current stage
            int bonusDamage = basePoints * monster.CurrentStage;
            monster.TakeDamage(bonusDamage);
            Debug.Log($"Bonus effect: Dealt {bonusDamage} damage (base {basePoints} x stage {monster.CurrentStage}).");
        }
    }

    /// <summary>
    /// Applies healing to the player.
    /// </summary>
    /// <param name="amount">Amount to heal.</param>
    /// <param name="player">Target player.</param>
    public static void ApplyHeal(int amount, Player player)
    {
        if (player != null)
        {
            player.Heal(amount);
            Debug.Log($"Heal effect: Healed player for {amount}.");
        }
    }
}
