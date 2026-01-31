using UnityEngine;
using System.Collections;

/// <summary>
/// Visual representation of the monster entity.
/// Handles color changes based on stage and damage effects.
/// </summary>
public class MonsterVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Monster monster;
    
    [Header("Visual Settings")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    [SerializeField] private Color damageFlashColor = Color.white;
    [SerializeField] private float stageTransitionDuration = 0.5f;
    
    private Renderer monsterRenderer;
    private Color currentStageColor;
    private bool isFlashing;

    /// <summary>
    /// Initialize references.
    /// </summary>
    private void Awake()
    {
        monsterRenderer = GetComponent<Renderer>();
        
        if (monster == null)
        {
            monster = GetComponent<Monster>();
        }
    }

    /// <summary>
    /// Subscribe to monster events.
    /// </summary>
    private void Start()
    {
        if (monster != null)
        {
            monster.OnDamageTaken += HandleDamageTaken;
            monster.OnStageChanged += HandleStageChanged;
            monster.OnDefeated += HandleDefeated;
            
            // Set initial color
            UpdateStageColor();
        }
    }

    /// <summary>
    /// Unsubscribe from events.
    /// </summary>
    private void OnDestroy()
    {
        if (monster != null)
        {
            monster.OnDamageTaken -= HandleDamageTaken;
            monster.OnStageChanged -= HandleStageChanged;
            monster.OnDefeated -= HandleDefeated;
        }
    }

    /// <summary>
    /// Sets the monster reference.
    /// </summary>
    /// <param name="monsterRef">Monster to visualize.</param>
    public void SetMonster(Monster monsterRef)
    {
        // Unsubscribe from old monster
        if (monster != null)
        {
            monster.OnDamageTaken -= HandleDamageTaken;
            monster.OnStageChanged -= HandleStageChanged;
            monster.OnDefeated -= HandleDefeated;
        }
        
        monster = monsterRef;
        
        // Subscribe to new monster
        if (monster != null)
        {
            monster.OnDamageTaken += HandleDamageTaken;
            monster.OnStageChanged += HandleStageChanged;
            monster.OnDefeated += HandleDefeated;
            
            UpdateStageColor();
        }
    }

    /// <summary>
    /// Updates the color based on current stage.
    /// </summary>
    private void UpdateStageColor()
    {
        if (monster == null || monsterRenderer == null) return;
        
        currentStageColor = monster.GetStageColor();
        monsterRenderer.material.color = currentStageColor;
    }

    /// <summary>
    /// Handles damage taken visual feedback.
    /// </summary>
    private void HandleDamageTaken(int currentHealth, int maxHealth)
    {
        if (!isFlashing)
        {
            StartCoroutine(DamageFlashCoroutine());
        }
    }

    /// <summary>
    /// Handles stage change visual transition.
    /// </summary>
    private void HandleStageChanged(int newStage)
    {
        StartCoroutine(StageTransitionCoroutine());
    }

    /// <summary>
    /// Handles monster defeat visual effect.
    /// </summary>
    private void HandleDefeated()
    {
        StartCoroutine(DefeatCoroutine());
    }

    /// <summary>
    /// Coroutine for damage flash effect.
    /// </summary>
    private IEnumerator DamageFlashCoroutine()
    {
        isFlashing = true;
        
        if (monsterRenderer != null)
        {
            Color originalColor = monsterRenderer.material.color;
            monsterRenderer.material.color = damageFlashColor;
            
            yield return new WaitForSeconds(damageFlashDuration);
            
            monsterRenderer.material.color = currentStageColor;
        }
        
        isFlashing = false;
    }

    /// <summary>
    /// Coroutine for stage transition effect.
    /// </summary>
    private IEnumerator StageTransitionCoroutine()
    {
        if (monster == null || monsterRenderer == null) yield break;
        
        Color oldColor = currentStageColor;
        Color newColor = monster.GetStageColor();
        
        // Shake effect
        Vector3 originalPosition = transform.position;
        float shakeIntensity = 0.2f;
        float shakeDuration = stageTransitionDuration;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shakeDuration;
            
            // Interpolate color
            monsterRenderer.material.color = Color.Lerp(oldColor, newColor, t);
            
            // Shake position
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeIntensity, shakeIntensity),
                Random.Range(-shakeIntensity, shakeIntensity),
                0
            );
            transform.position = originalPosition + shakeOffset * (1 - t);
            
            yield return null;
        }
        
        // Ensure final state
        transform.position = originalPosition;
        currentStageColor = newColor;
        monsterRenderer.material.color = currentStageColor;
        
        Debug.Log($"Monster transitioned to stage {monster.CurrentStage}");
    }

    /// <summary>
    /// Coroutine for defeat effect.
    /// </summary>
    private IEnumerator DefeatCoroutine()
    {
        if (monsterRenderer == null) yield break;
        
        // Flash rapidly
        for (int i = 0; i < 5; i++)
        {
            monsterRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            monsterRenderer.material.color = currentStageColor;
            yield return new WaitForSeconds(0.1f);
        }
        
        // Fade out
        float fadeDuration = 1f;
        float elapsed = 0f;
        Color startColor = monsterRenderer.material.color;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            Color fadeColor = startColor;
            fadeColor.a = 1 - t;
            monsterRenderer.material.color = fadeColor;
            
            // Shrink
            transform.localScale = Vector3.one * (1 - t * 0.5f);
            
            yield return null;
        }
        
        Debug.Log("Monster defeat animation complete");
    }

    /// <summary>
    /// Forces a visual refresh.
    /// </summary>
    public void RefreshVisuals()
    {
        UpdateStageColor();
    }
}
