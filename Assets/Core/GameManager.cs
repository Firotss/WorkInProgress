using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    #region Singleton

    private static GameManager instance;
    private static bool applicationIsQuitting;

    public static GameManager Instance
    {
        get
        {
            if (applicationIsQuitting)
                return null;

            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();

                if (instance == null && Application.isPlaying)
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

    // Публичное свойство для InputHandler
    public CardVisual SelectedBoardCard => SelectedBoardCardForWithdraw;
    public CardVisual SelectedBoardCardForWithdraw { get; private set; }

    #endregion

    #region Events
    
    public event Action<GameState> OnGameStateChanged;
    public event Action OnGameStarted;
    public event Action<bool> OnGameEnded;
    
    #endregion

    #region Variables (Turn Limits)
    
    private int cardsPlayedThisTurn = 0;
    private const int MAX_CARDS_PER_TURN = 3;
    
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
        applicationIsQuitting = false;
        CurrentState = GameState.NotStarted;
        if (GetComponent<SoundManager>() == null)
            gameObject.AddComponent<SoundManager>();
    }

    private void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void Start()
    {
        FindRequiredComponents();
    }
    
    #endregion

    #region Initialization
    
    private void FindRequiredComponents()
    {
        if (player == null) player = FindObjectOfType<Player>();
        if (monster == null) monster = FindObjectOfType<Monster>();
        
        BoardManager[] boards = FindObjectsOfType<BoardManager>();
        foreach (var board in boards)
        {
            if (board.IsPlayerBoard && playerBoard == null) playerBoard = board;
            else if (!board.IsPlayerBoard && enemyBoard == null) enemyBoard = board;
        }
            
        if (hand == null) hand = FindObjectOfType<Hand>();
        
        Deck[] decks = FindObjectsOfType<Deck>();
        if (decks.Length >= 2) { playerDeck = decks[0]; enemyDeck = decks[1]; }
        else if (decks.Length == 1) { playerDeck = decks[0]; }
            
        if (turnManager == null) turnManager = FindObjectOfType<TurnManager>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (enemyAI == null) enemyAI = FindObjectOfType<EnemyAI>();
    }

    public void StartGame()
    {
        Debug.Log("=== Starting New Game ===");
        FindRequiredComponents();
        
        if (!ValidateComponents()) return;
        
        InitializeGame();
        
        SetGameState(GameState.PlayerTurn);
        OnGameStarted?.Invoke();
        
        // Сброс счетчика перед первым ходом
        StartNewTurn(); 
        
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
        if (turnManager == null) { Debug.LogError("TurnManager not found!"); valid = false; }
        
        return valid;
    }

    private void InitializeGame()
    {
        player.ResetPlayer();
        monster.ResetMonster();

        hand.SetDeck(playerDeck);
        hand.SetPlayerBoard(playerBoard);
        playerDeck.ResetDeck();
        hand.ClearHand();
        hand.DrawInitialCards();

        if (enemyDeck != null)
            enemyDeck.ResetDeck();
        playerBoard.SetDeck(playerDeck);
        if (enemyBoard != null)
            enemyBoard.SetDeck(enemyDeck);
        if (enemyAI != null)
            enemyAI.Initialize(enemyBoard, enemyDeck);
        playerBoard.ClearBoard();
        if (enemyBoard != null)
            enemyBoard.ClearBoard();

        turnManager.Initialize(this, playerBoard, enemyBoard, player, monster, hand, enemyAI);
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
            case GameState.Victory: HandleVictory(); break;
            case GameState.GameOver: HandleGameOver(); break;
        }
    }

    private void HandleVictory()
    {
        Debug.Log("=== VICTORY! ===");
        OnGameEnded?.Invoke(true);
        if (uiManager != null) uiManager.ShowGameEndScreen(true);
    }

    private void HandleGameOver()
    {
        Debug.Log("=== GAME OVER! ===");
        OnGameEnded?.Invoke(false);
        if (uiManager != null) uiManager.ShowGameEndScreen(false);
    }
    
    #endregion

    #region Card Placement & Turn Logic
    
    // ВАЖНО: Этот метод должен вызываться из TurnManager при старте хода игрока!
    public void StartNewTurn()
    {
        cardsPlayedThisTurn = 0;
        Debug.Log($"<color=green>НОВЫЙ ХОД. Счетчик сброшен: 0/{MAX_CARDS_PER_TURN}</color>");
    }

    public void TryPlaceCardInColumn(int column)
    {
        if (CurrentState != GameState.PlayerTurn) return;

        // 1. ПРОВЕРКА ЛИМИТА (ФИКС)
        if (cardsPlayedThisTurn >= MAX_CARDS_PER_TURN)
        {
            Debug.LogWarning($"<color=red>Лимит карт исчерпан! ({cardsPlayedThisTurn}/{MAX_CARDS_PER_TURN})</color>");
            return;
        }

        CardVisual selectedCard = hand.SelectedCard;
        if (selectedCard == null) return;

        if (playerBoard.PlaceCard(selectedCard, column))
        {
            hand.RemoveCard(selectedCard);
            
            // 2. УВЕЛИЧИВАЕМ СЧЕТЧИК
            cardsPlayedThisTurn++;
            
            turnManager?.RecordCardPlaced();
            SelectedBoardCardForWithdraw = null;
            SoundManager.Instance?.PlayCardPlaced();
            Debug.Log($"Карта сыграна. Счетчик: {cardsPlayedThisTurn}/{MAX_CARDS_PER_TURN}");
        }
    }

    public void TryWithdrawCard()
    {
        if (CurrentState != GameState.PlayerTurn) return;

        CardVisual toWithdraw = SelectedBoardCardForWithdraw;
        if (toWithdraw == null) return;

        if (playerBoard == null || hand == null || !playerBoard.TryRemoveCardFromRow0(toWithdraw))
            return;

        hand.AddCardBack(toWithdraw);
        
        // 3. УМЕНЬШАЕМ СЧЕТЧИК (возвращаем ход)
        if (cardsPlayedThisTurn > 0) cardsPlayedThisTurn--;
        
        turnManager?.RecordCardWithdrawn();
        
        // Снимаем выделение, так как карта теперь в руке
        DeselectBoardCard();

        SoundManager.Instance?.PlayCardWithdrawn();
        Debug.Log($"Карта возвращена. Счетчик: {cardsPlayedThisTurn}/{MAX_CARDS_PER_TURN}");
    }

    // Для управления через клавишу "E"
    public void TryPlaceSelectedCard()
    {
        if (CurrentState != GameState.PlayerTurn) return;

        if (cardsPlayedThisTurn >= MAX_CARDS_PER_TURN)
        {
            Debug.LogWarning("Лимит карт исчерпан!");
            return;
        }

        CardVisual selectedCard = hand.SelectedCard;
        if (selectedCard == null) return;

        if (playerBoard.PlaceCardAuto(selectedCard))
        {
            hand.RemoveCard(selectedCard);
            cardsPlayedThisTurn++; // Тоже увеличиваем счетчик
            turnManager?.RecordCardPlaced();
            SoundManager.Instance?.PlayCardPlaced();
            Debug.Log($"Карта сыграна (Авто). Счетчик: {cardsPlayedThisTurn}/{MAX_CARDS_PER_TURN}");
        }
    }

    public void SetSelectedBoardCardForWithdraw(CardVisual card)
    {
        // ЛОГИКА ТУМБЛЕРА (ФИКС)
        // Если кликнули по той же карте -> снимаем выделение
        if (SelectedBoardCardForWithdraw == card)
        {
            DeselectBoardCard();
            return;
        }

        // Если была выбрана другая -> гасим её
        if (SelectedBoardCardForWithdraw != null)
        {
            SelectedBoardCardForWithdraw.SetSelected(false);
        }

        SelectedBoardCardForWithdraw = card;
        
        if (SelectedBoardCardForWithdraw != null)
        {
            SelectedBoardCardForWithdraw.SetSelected(true);
            Debug.Log($"Карта на столе выбрана: {card.CardData.CardName}");
        }
    }

    // Метод для InputHandler, чтобы снимать выделение при клике в пустоту
    public void DeselectBoardCard()
    {
        if (SelectedBoardCardForWithdraw != null)
        {
            SelectedBoardCardForWithdraw.SetSelected(false);
            SelectedBoardCardForWithdraw = null;
            Debug.Log("Выделение с карты на столе снято.");
        }
    }

    public bool IsCardOnPlayerRow0(CardVisual card)
    {
        return playerBoard != null && card != null && playerBoard.GetCardRow(card) == 0;
    }

    public void OnEndTurnClicked()
    {
        if (turnManager != null && CurrentState == GameState.PlayerTurn)
        {
            SoundManager.Instance?.PlayEndTurn();
            turnManager.EndTurn();
        }
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