using UnityEngine;

/// <summary>
/// Player entity representing the human player in the card game.
/// Manages health, defense, and hand reference.
/// </summary>
public class Player : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 100;
    
    public string PlayerName { get; set; }
    public int Health { get; private set; }
    public int MaxHealth => maxHealth;
    public int Defense { get; private set; }
    public Hand Hand { get; set; }

    /// <summary>
    /// Initializes the player with default values.
    /// </summary>
    private void Awake()
    {
        PlayerName = "Player";
        Health = maxHealth;
        Defense = 0;
    }

    /// <summary>
    /// Applies damage to the player, reduced by defense.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    public void TakeDamage(int damage)
    {
        // Defense absorbs damage first
        int effectiveDamage = Mathf.Max(0, damage - Defense);
        Health -= effectiveDamage;
        
        // Reset defense after taking damage
        Defense = 0;
        
        if (Health <= 0)
        {
            Health = 0;
            OnPlayerDeath();
        }
        
        Debug.Log($"Player took {effectiveDamage} damage. Health: {Health}");
    }

    /// <summary>
    /// Adds defense points to the player.
    /// </summary>
    /// <param name="amount">Amount of defense to add.</param>
    public void AddDefense(int amount)
    {
        Defense += amount;
        Debug.Log($"Player gained {amount} defense. Total defense: {Defense}");
    }

    /// <summary>
    /// Heals the player by the specified amount.
    /// </summary>
    /// <param name="amount">Amount to heal.</param>
    public void Heal(int amount)
    {
        Health = Mathf.Min(Health + amount, maxHealth);
        Debug.Log($"Player healed {amount}. Health: {Health}");
    }

    /// <summary>
    /// Called when player health reaches zero.
    /// </summary>
    private void OnPlayerDeath()
    {
        Debug.Log("Player has died! Game Over.");
        GameManager.Instance?.OnPlayerDeath();
    }

    /// <summary>
    /// Resets player to full health for a new game.
    /// </summary>
    public void ResetPlayer()
    {
        Health = maxHealth;
        Defense = 0;
    }
}
