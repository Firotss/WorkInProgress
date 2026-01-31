using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    #region Singleton
    
    private static GameManager instance;
    
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }
    
    #endregion

    #region References
    
    [Header("Game Components")]
    [SerializeField] private Player player;
    [SerializeField] private Monster monster;
    [SerializeField] private BoardManager playerBoard;
    [SerializeField] private BoardManager enemyBoard;
    [SerializeField] private Hand hand;
    [SerializeField] private Deck playerDeck;
    [SerializeField] private Deck enemyDeck;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private EnemyAI enemyAI;
    
    #endregion

    #region Properties
    
    public GameState CurrentState { get; private set; }
    public Player Player => player;
    public Monster Monster => monster;
    public BoardManager Board => playerBoard;
    public BoardManager PlayerBoard => playerBoard;
    public BoardManager EnemyBoard => enemyBoard;
    public Hand PlayerHand => hand;
    public EnemyAI EnemyAI => enemyAI;
    
    #endregion

    #region Events
    
    public event Action<GameState> OnGameStateChanged;
    public event Action OnGameStarted;
    public event Action<bool> OnGameEnded;
    
    #endregion

    #region Unity Lifecycle
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        CurrentState = GameState.NotStarted;
    }

    private void Start()
    {
        FindRequiredComponents();
    }
    
    #endregion

    #region Initialization
    
    private void FindRequiredComponents()
    {
        if (player == null)
            player = FindObjectOfType<Player>();
            
        if (monster == null)
            monster = FindObjectOfType<Monster>();
        
        // Find boards
        BoardManager[] boards = FindObjectsOfType<BoardManager>();
        foreach (var board in boards)
        {
            if (board.IsPlayerBoard && playerBoard == null)
                playerBoard = board;
            else if (!board.IsPlayerBoard && enemyBoard == null)
                enemyBoard = board;
        }
            
        if (hand == null)
            hand = FindObjectOfType<Hand>();
        
        // Find decks
        Deck[] decks = FindObjectsOfType<Deck>();
        if (decks.Length >= 2)
        {
            playerDeck = decks[0];
            enemyDeck = decks[1];
        }
        else if (decks.Length == 1)
        {
            playerDeck = decks[0];
        }
            
        if (turnManager == null)
            turnManager = FindObjectOfType<TurnManager>();
            
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
            
        if (enemyAI == null)
            enemyAI = FindObjectOfType<EnemyAI>();
    }

    public void StartGame()
    {
        Debug.Log("=== Starting New Game ===");
        
        FindRequiredComponents();
        
        if (!ValidateComponents())
        {
            Debug.LogError("Cannot start game - missing required components!");
            return;
        }
        
        InitializeGame();
        
        SetGameState(GameState.PlayerTurn);
        OnGameStarted?.Invoke();
        
        turnManager.StartTurn();
    }

    private bool ValidateComponents()
    {
        bool valid = true;
        
        if (player == null) { Debug.LogError("Player not found!"); valid = false; }
        if (monster == null) { Debug.LogError("Monster not found!"); valid = false; }
        if (playerBoard == null) { Debug.LogError("Player Board not found!"); valid = false; }
        if (enemyBoard == null) { Debug.LogError("Enemy Board not found!"); valid = false; }
        if (hand == null) { Debug.LogError("Hand not found!"); valid = false; }
        if (playerDeck == null) { Debug.LogError("Player Deck not found!"); valid = false; }
        if (enemyDeck == null) { Debug.LogError("Enemy Deck not found!"); valid = false; }
        if (turnManager == null) { Debug.LogError("TurnManager not found!"); valid = false; }
        if (enemyAI == null) { Debug.LogError("EnemyAI not found!"); valid = false; }
        
        return valid;
    }

    private void InitializeGame()
    {
        // Reset entities
        player.ResetPlayer();
        monster.ResetMonster();
        
        // Set up player deck and hand (reference: FillCards, dynamic max hand)
        hand.SetDeck(playerDeck);
        hand.SetPlayerBoard(playerBoard);
        playerDeck.ResetDeck();
        hand.ClearHand();
        hand.DrawInitialCards(); // Fill to max (5 + board "max hand increase")

        // Set up enemy deck
        enemyDeck.ResetDeck();

        // Link boards to decks for discarding activated cards
        playerBoard.SetDeck(playerDeck);
        enemyBoard.SetDeck(enemyDeck);

        // Initialize enemy AI
        enemyAI.Initialize(enemyBoard, enemyDeck);

        // Clear boards
        playerBoard.ClearBoard();
        enemyBoard.ClearBoard();
        
        // Initialize turn manager
        turnManager.Initialize(this, playerBoard, enemyBoard, player, monster, hand, enemyAI);
        
        // Link player to hand
        player.Hand = hand;
        
        Debug.Log("Game initialized successfully.");
    }
    
    #endregion

    #region Game State
    
    public void SetGameState(GameState newState)
    {
        if (CurrentState == newState) return;
        
        GameState oldState = CurrentState;
        CurrentState = newState;
        
        Debug.Log($"Game state changed: {oldState} -> {newState}");
        OnGameStateChanged?.Invoke(newState);
        
        switch (newState)
        {
            case GameState.Victory:
                HandleVictory();
                break;
            case GameState.GameOver:
                HandleGameOver();
                break;
        }
    }

    private void HandleVictory()
    {
        Debug.Log("=== VICTORY! All masks destroyed! ===");
        OnGameEnded?.Invoke(true);
        
        if (uiManager != null)
        {
            uiManager.ShowGameEndScreen(true);
        }
    }

    private void HandleGameOver()
    {
        Debug.Log("=== GAME OVER! Player defeated! ===");
        OnGameEnded?.Invoke(false);
        
        if (uiManager != null)
        {
            uiManager.ShowGameEndScreen(false);
        }
    }
    
    #endregion

    #region Card Placement
    
    public void TryPlaceSelectedCard()
    {
        if (CurrentState != GameState.PlayerTurn)
        {
            Debug.LogWarning("Cannot place cards - not player's turn!");
            return;
        }
        
        CardVisual selectedCard = hand.SelectedCard;
        
        if (selectedCard == null)
        {
            Debug.LogWarning("No card selected!");
            return;
        }
        
        if (playerBoard.PlaceCardAuto(selectedCard))
        {
            hand.RemoveCard(selectedCard);
            Debug.Log($"Card placed on board: {selectedCard.CardData.CardName}");
        }
    }

    public void OnEndTurnClicked()
    {
        if (turnManager != null && CurrentState == GameState.PlayerTurn)
            turnManager.EndTurn();
    }
    
    #endregion

    #region Game Events
    
    public void OnPlayerDeath()
    {
        SetGameState(GameState.GameOver);
    }

    public void OnMonsterDefeated()
    {
        SetGameState(GameState.Victory);
    }

    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        turnManager.Reset();
        StartGame();
    }
    
    #endregion
}
