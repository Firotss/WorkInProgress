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
            if (applicationIsQuitting) return null;

            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
                if (instance == null && Application.isPlaying)
                {
                    // ВАЖНО: Не създаваме нов обект автоматично, защото ще е празен!
                    Debug.LogError("Грешка: GameManager липсва в сцената!"); 
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
    
    // Поправка на грешките в Player.cs и UIManager.cs
    public Player Player => player;
    public Monster Monster => monster;
    
    // Добавени са липсващите свойства за UIManager
    public BoardManager Board => playerBoard; 
    public BoardManager PlayerBoard => playerBoard; 
    public BoardManager EnemyBoard => enemyBoard;   
    
    public Hand PlayerHand => hand;
    public EnemyAI EnemyAI => enemyAI;

    // Свойство за InputHandler
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
        // Опитваме да намерим компонентите, ако са забравени в инспектора
        if (!ValidateComponents())
        {
            FindRequiredComponents();
        }
    }
    
    #endregion

    #region Initialization
    
    private void FindRequiredComponents()
    {
        if (player == null) player = FindObjectOfType<Player>();
        if (monster == null) monster = FindObjectOfType<Monster>();
        
        // --- УМНЫЙ ПОИСК ПОЛЕЙ ---
        if (playerBoard == null || enemyBoard == null)
        {
            BoardManager[] boards = FindObjectsOfType<BoardManager>();
            foreach (var board in boards)
            {
                // Проверяем имя объекта или позицию, если нет флага IsPlayerBoard
                // Обычно поле игрока находится ниже (Y меньше) или называется "PlayerBoard"
                if (board.name.Contains("Player") || board.transform.position.z < 0 || board.transform.position.y < 0) 
                {
                    playerBoard = board;
                }
                else
                {
                    enemyBoard = board;
                }
            }
        }
        // -------------------------

        if (hand == null) hand = FindObjectOfType<Hand>();
        
        // --- УМНЫЙ ПОИСК КОЛОД ---
        if (playerDeck == null || enemyDeck == null)
        {
            Deck[] decks = FindObjectsOfType<Deck>();
            foreach (var deck in decks)
            {
                if (deck.name.Contains("Player") || deck.transform.position.z < 0) 
                    playerDeck = deck;
                else 
                    enemyDeck = deck;
            }
        }
        // -------------------------

        if (turnManager == null) turnManager = FindObjectOfType<TurnManager>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (enemyAI == null) enemyAI = FindObjectOfType<EnemyAI>();
    }

    public void StartGame()
    {
        Debug.Log("=== Starting New Game ===");
        
        FindRequiredComponents();
        if (!ValidateComponents()) 
        {
            Debug.LogError("Играта не може да започне, липсват компоненти!");
            return;
        }
        
        InitializeGame();
        
        SetGameState(GameState.PlayerTurn);
        OnGameStarted?.Invoke();
        
        StartNewTurn(); 
        turnManager.StartTurn();
    }

    private bool ValidateComponents()
    {
        bool valid = true;
        if (player == null) { Debug.LogError("Липсва Player!"); valid = false; }
        if (monster == null) { Debug.LogError("Липсва Monster!"); valid = false; }
        if (playerBoard == null) { Debug.LogError("Липсва PlayerBoard!"); valid = false; }
        if (enemyBoard == null) { Debug.LogError("Липсва EnemyBoard!"); valid = false; }
        if (hand == null) { Debug.LogError("Липсва Hand!"); valid = false; }
        if (playerDeck == null) { Debug.LogError("Липсва PlayerDeck!"); valid = false; }
        if (turnManager == null) { Debug.LogError("Липсва TurnManager!"); valid = false; }
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

        if (enemyDeck != null) enemyDeck.ResetDeck();
        
        playerBoard.SetDeck(playerDeck);
        enemyBoard.SetDeck(enemyDeck);
        
        if (enemyAI != null) enemyAI.Initialize(enemyBoard, enemyDeck);
        
        playerBoard.ClearBoard();
        enemyBoard.ClearBoard();
        
        turnManager.Initialize(this, playerBoard, enemyBoard, player, monster, hand, enemyAI);
        player.Hand = hand;
        
        Debug.Log("Game initialized successfully.");
    }
    
    #endregion

    #region Game State & Events
    
    public void SetGameState(GameState newState)
    {
        if (CurrentState == newState) return;
        
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
        
        if (newState == GameState.Victory) HandleVictory();
        if (newState == GameState.GameOver) HandleGameOver();
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

    // Тези методи липсваха и предизвикваха грешките
    public void OnPlayerDeath()
    {
        SetGameState(GameState.GameOver);
    }

    public void OnMonsterDefeated()
    {
        SetGameState(GameState.Victory);
    }
    
    #endregion

    #region Card Placement & Turn Logic
    
    public void StartNewTurn()
    {
        cardsPlayedThisTurn = 0;
        DeselectBoardCard();
    }

    public void TryPlaceCardInColumn(int column)
    {
        if (CurrentState != GameState.PlayerTurn) return;

        if (cardsPlayedThisTurn >= MAX_CARDS_PER_TURN)
        {
            Debug.LogWarning("Лимитът за карти този ход е изчерпан!");
            return;
        }

        CardVisual selectedCard = hand.SelectedCard;
        if (selectedCard == null) return;

        if (playerBoard.PlaceCard(selectedCard, column))
        {
            hand.RemoveCard(selectedCard);
            cardsPlayedThisTurn++;
            turnManager?.RecordCardPlaced();
            hand.DeselectCard();
            SoundManager.Instance?.PlayCardPlaced();
        }
    }

    public void TryWithdrawCard()
    {
        if (CurrentState != GameState.PlayerTurn) return;

        CardVisual toWithdraw = SelectedBoardCardForWithdraw;
        if (toWithdraw == null) return;

        if (playerBoard.TryRemoveCardFromRow0(toWithdraw))
        {
            hand.AddCardBack(toWithdraw);
            if (cardsPlayedThisTurn > 0) cardsPlayedThisTurn--;
            
            turnManager?.RecordCardWithdrawn();
            DeselectBoardCard();
            SoundManager.Instance?.PlayCardWithdrawn();
        }
    }

    public void SetSelectedBoardCardForWithdraw(CardVisual card)
    {
        if (SelectedBoardCardForWithdraw == card)
        {
            DeselectBoardCard();
            return;
        }

        if (SelectedBoardCardForWithdraw != null)
            SelectedBoardCardForWithdraw.SetSelected(false);

        SelectedBoardCardForWithdraw = card;
        
        if (SelectedBoardCardForWithdraw != null)
            SelectedBoardCardForWithdraw.SetSelected(true);
    }

    public void DeselectBoardCard()
    {
        if (SelectedBoardCardForWithdraw != null)
        {
            SelectedBoardCardForWithdraw.SetSelected(false);
            SelectedBoardCardForWithdraw = null;
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
    
    // За управление с клавиатура/бутони
    public void TryPlaceSelectedCard() { /* Логика при нужда */ }

    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        turnManager.Reset();
        StartGame();
    }
    
    #endregion
}