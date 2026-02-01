using UnityEngine;
using System;

/// <summary>
/// Monster entity with 3 stages (masks).
/// Each stage has its own health pool and visual appearance.
/// When health reaches 0, the monster transitions to the next stage.
/// </summary>
public class Monster : MonoBehaviour
{
    [Header("Monster Stats")]
    [SerializeField] private int healthPerStage = 100;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private int totalStages = 3;
    
    [Header("Stage Sprites")]
    [SerializeField] private Sprite stage1Sprite;  // Green
    [SerializeField] private Sprite stage2Sprite;  // Yellow
    [SerializeField] private Sprite stage3Sprite;  // Red
    [SerializeField] private Sprite defaultSprite;  // Red

    /// <summary>
    /// Current health of the monster in this stage.
    /// </summary>
    public int CurrentHealth { get; private set; }
    
    /// <summary>
    /// Maximum health per stage.
    /// </summary>
    public int MaxHealth => healthPerStage;
    
    /// <summary>
    /// Current stage (1-3). Each stage is a different "mask".
    /// </summary>
    public int CurrentStage { get; private set; }
    
    /// <summary>
    /// Total number of stages.
    /// </summary>
    public int TotalStages => totalStages;
    
    /// <summary>
    /// Attack damage dealt to player each turn.
    /// </summary>
    public int AttackDamage => attackDamage;
    
    /// <summary>
    /// Whether the monster has been fully defeated.
    /// </summary>
    public bool IsDefeated => CurrentStage > totalStages;

    /// <summary>
    /// Event fired when stage changes.
    /// </summary>
    public event Action<int> OnStageChanged;
    
    /// <summary>
    /// Event fired when monster is fully defeated.
    /// </summary>
    public event Action OnDefeated;
    
    /// <summary>
    /// Event fired when monster takes damage.
    /// </summary>
    public event Action<int, int> OnDamageTaken; // current health, max health

    private SpriteRenderer monsterRenderer;
    private MonsterVisual monsterVisual;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    /// <summary>
    /// Initialize monster at stage 1.
    /// </summary>
    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialScale = transform.localScale;
        CurrentStage = 1;
        CurrentHealth = healthPerStage;
        monsterRenderer = GetComponent<SpriteRenderer>();
        monsterVisual = GetComponent<MonsterVisual>();
        UpdateVisualAppearance();
    }

    /// <summary>
    /// Applies damage to the monster.
    /// If health drops to 0, transitions to next stage.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        Debug.Log($"Monster took {damage} damage. Stage {CurrentStage} Health: {CurrentHealth}/{healthPerStage}");
        
        OnDamageTaken?.Invoke(CurrentHealth, healthPerStage);
        
        if (CurrentHealth <= 0)
        {
            SwitchToNextStage();
        }
    }

    /// <summary>
    /// Transitions to the next stage (mask breaks).
    /// </summary>
    private void SwitchToNextStage()
    {
        CurrentStage++;
        
        if (CurrentStage > totalStages)
        {
            // Monster fully defeated
            Debug.Log("Monster defeated! All masks destroyed!");
            OnDefeated?.Invoke();
            GameManager.Instance?.OnMonsterDefeated();
        }
        else
        {
            // Reset health for new stage
            CurrentHealth = healthPerStage;
            Debug.Log($"Mask cracked! Stage {CurrentStage} begins. Health restored to {healthPerStage}.");
            
            UpdateVisualAppearance();
            OnStageChanged?.Invoke(CurrentStage);
        }
    }

    /// <summary>
    /// Updates the monster's visual appearance based on current stage.
    /// </summary>
    private void UpdateVisualAppearance()
    {
        if (monsterRenderer == null)
        {
            monsterRenderer = GetComponent<SpriteRenderer>();
        }
        if (monsterRenderer != null)
        {
            Sprite stageColor = GetStageColor();
            monsterRenderer.sprite = stageColor;
        }
    }

    /// <summary>
    /// Gets the color for the current stage.
    /// </summary>
    /// <returns>Color for the current stage.</returns>
    public Sprite GetStageColor()
    {
        return CurrentStage switch
        {
            1 => stage1Sprite,
            2 => stage2Sprite,
            3 => stage3Sprite,
            _ => defaultSprite
        };
    }

    /// <summary>
    /// Monster attacks the player.
    /// </summary>
    /// <param name="player">Target player.</param>
    public void AttackPlayer(Player player)
    {
        if (player != null && !IsDefeated)
        {
            // Attack damage scales slightly with stage
            int scaledDamage = attackDamage + (CurrentStage - 1) * 5;
            player.TakeDamage(scaledDamage);
            Debug.Log($"Monster attacks player for {scaledDamage} damage!");
        }
    }

    /// <summary>
    /// Resets the monster to initial state.
    /// </summary>
    public void ResetMonster()
    {
        CurrentStage = 1;
        CurrentHealth = healthPerStage;
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        transform.localScale = initialScale;
        UpdateVisualAppearance();

        // Stop any running visual animations and reset visuals
        if (monsterVisual != null)
            monsterVisual.ResetVisuals();
    }

    /// <summary>
    /// Forces visual update (useful after scene load).
    /// </summary>
    public void RefreshVisuals()
    {
        UpdateVisualAppearance();
    }
}
