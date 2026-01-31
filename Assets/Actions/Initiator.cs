using UnityEngine;

/// <summary>
/// Initiates the start of the game.
/// Attach this to an empty GameObject in the scene.
/// Will either use GameSetup for auto-setup or start an existing GameManager.
/// </summary>
public class Initiator : MonoBehaviour
{
    [Header("Setup Options")]
    [SerializeField] private bool useAutoSetup = true;
    [SerializeField] private bool autoStartGame = false;
    [SerializeField] private float startDelay = 0.5f;
    
    [Header("References (Optional - for manual setup)")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private GameObject boardPrefab;
    
    /// <summary>
    /// Called when the game starts.
    /// Initializes all game components.
    /// </summary>
    private void Start()
    {
        if (useAutoSetup)
        {
            // Check if GameSetup exists, if not create it
            GameSetup setup = FindObjectOfType<GameSetup>();
            if (setup == null)
            {
                GameObject setupObj = new GameObject("GameSetup");
                setup = setupObj.AddComponent<GameSetup>();
            }
            
            // Add input handler
            if (FindObjectOfType<InputHandler>() == null)
            {
                gameObject.AddComponent<InputHandler>();
            }
            
            if (autoStartGame)
                Invoke(nameof(StartGameDelayed), startDelay);
        }
        else
        {
            if (autoStartGame)
                StartGameDelayed();
        }
    }

    /// <summary>
    /// Starts the game after a short delay to allow setup to complete.
    /// </summary>
    private void StartGameDelayed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
            Debug.Log("Game started successfully!");
        }
        else
        {
            Debug.LogError("GameManager not found! Make sure GameManager is in the scene or use auto-setup.");
        }
    }
}
