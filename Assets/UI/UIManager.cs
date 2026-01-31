using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Health Displays")]
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI monsterHealthText;
    [SerializeField] private TextMeshProUGUI monsterStageText;
    
    [Header("Turn Info")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI gameStateText;
    
    [Header("Buttons")]
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button restartButton;
    
    [Header("Hand Info")]
    [SerializeField] private TextMeshProUGUI handCountText;
    [SerializeField] private TextMeshProUGUI deckCountText;
    
    [Header("Start Screen")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startButton;

    [Header("Game End Screen")]
    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TextMeshProUGUI gameEndText;
    [SerializeField] private TextMeshProUGUI gameEndSubtext;
    
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Monster monster;
    [SerializeField] private Hand hand;
    [SerializeField] private Deck deck;
    [SerializeField] private TurnManager turnManager;

    private void Start()
    {
        FindReferences();
        FindUIElements();
        SetupButtonListeners();
        SubscribeToEvents();
        
        if (gameEndPanel != null)
            gameEndPanel.SetActive(false);

        if (startPanel != null)
            startPanel.SetActive(GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.NotStarted);

        UpdateAllUI();
    }

    private void FindReferences()
    {
        if (player == null) player = FindObjectOfType<Player>();
        if (monster == null) monster = FindObjectOfType<Monster>();
        if (hand == null) hand = FindObjectOfType<Hand>();
        if (deck == null) deck = FindObjectOfType<Deck>();
        if (turnManager == null) turnManager = FindObjectOfType<TurnManager>();
    }

    private void FindUIElements()
    {
        if (playerHealthText == null)
            playerHealthText = FindTextByName("PlayerHealthText");
        if (monsterHealthText == null)
            monsterHealthText = FindTextByName("MonsterHealthText");
        if (monsterStageText == null)
            monsterStageText = FindTextByName("MonsterStageText");
        if (turnText == null)
            turnText = FindTextByName("TurnText");
        if (roundText == null)
            roundText = FindTextByName("RoundText");
        if (gameStateText == null)
            gameStateText = FindTextByName("GameStateText");
        if (handCountText == null)
            handCountText = FindTextByName("HandCountText");
        if (deckCountText == null)
            deckCountText = FindTextByName("DeckCountText");
        if (gameEndText == null)
            gameEndText = FindTextByName("GameEndText");
        if (gameEndSubtext == null)
            gameEndSubtext = FindTextByName("GameEndSubtext");
        if (startPanel == null)
        {
            Transform p = transform.Find("StartPanel");
            if (p != null)
                startPanel = p.gameObject;
        }
        if (startButton == null)
            startButton = FindButtonByName("StartButton");

        // Find buttons by name
        if (endTurnButton == null)
            endTurnButton = FindButtonByName("EndTurnButton");
        if (restartButton == null)
            restartButton = FindButtonByName("RestartButton");
        if (restartButton == null && gameEndPanel != null)
            restartButton = gameEndPanel.GetComponentInChildren<Button>(true);

        if (gameEndPanel == null)
        {
            Transform panel = transform.Find("GameEndPanel");
            if (panel != null)
                gameEndPanel = panel.gameObject;
        }
    }

    private TextMeshProUGUI FindTextByName(string name)
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in texts)
        {
            if (text.gameObject.name == name)
                return text;
        }
        return null;
    }

    private Button FindButtonByName(string name)
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (var button in buttons)
        {
            if (button.gameObject.name == name)
                return button;
        }
        return null;
    }

    private void SetupButtonListeners()
    {
        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveAllListeners();
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
            Debug.Log("End Turn button wired up!");
        }
        else
        {
            Debug.LogWarning("End Turn button not found!");
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartClicked);
        }
    }

    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
        
        if (monster != null)
        {
            monster.OnDamageTaken += OnMonsterDamaged;
            monster.OnStageChanged += OnMonsterStageChanged;
        }
        
        if (hand != null)
        {
            hand.OnHandUpdated += OnHandUpdated;
        }
        
        if (turnManager != null)
        {
            turnManager.OnTurnStarted += OnTurnStarted;
            turnManager.OnRoundChanged += OnRoundChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
        
        if (monster != null)
        {
            monster.OnDamageTaken -= OnMonsterDamaged;
            monster.OnStageChanged -= OnMonsterStageChanged;
        }
        
        if (hand != null)
        {
            hand.OnHandUpdated -= OnHandUpdated;
        }
        
        if (turnManager != null)
        {
            turnManager.OnTurnStarted -= OnTurnStarted;
            turnManager.OnRoundChanged -= OnRoundChanged;
        }
    }

    public void UpdateAllUI()
    {
        UpdatePlayerHealthUI();
        UpdateMonsterHealthUI();
        UpdateHandUI();
        UpdateTurnUI();
    }

    public void UpdatePlayerHealthUI()
    {
        if (player == null) return;
        
        if (playerHealthText != null)
        {
            string text = $"Player HP: {player.Health}/{player.MaxHealth}";
            if (player.Defense > 0)
            {
                text += $" [DEF: {player.Defense}]";
            }
            playerHealthText.text = text;
        }
    }

    public void UpdateMonsterHealthUI()
    {
        if (monster == null) return;
        
        if (monsterHealthText != null)
        {
            monsterHealthText.text = $"Monster HP: {monster.CurrentHealth}/{monster.MaxHealth}";
        }
        
        if (monsterStageText != null)
        {
            monsterStageText.text = $"Mask {monster.CurrentStage}/{monster.TotalStages}";
        }
    }

    public void UpdateHandUI()
    {
        if (handCountText != null && hand != null)
        {
            handCountText.text = $"Hand: {hand.CardCount}/{hand.MaxHandSize}";
        }
        
        if (deckCountText != null && deck != null)
        {
            deckCountText.text = $"Deck: {deck.CardsRemaining}";
        }
    }

    public void UpdateTurnUI()
    {
        if (turnManager == null) return;
        
        if (turnText != null)
        {
            turnText.text = $"Turn: {turnManager.CurrentTurn}";
        }
        
        if (roundText != null)
        {
            roundText.text = $"Round: {turnManager.CurrentRound}";
        }
        
        UpdateGameStateUI();
    }

    private void UpdateGameStateUI()
    {
        if (gameStateText == null) return;
        
        if (GameManager.Instance == null)
        {
            gameStateText.text = "Waiting...";
            return;
        }
        
        gameStateText.text = GameManager.Instance.CurrentState switch
        {
            GameState.PlayerTurn => "YOUR TURN - Click cards, then END TURN",
            GameState.ProcessingTurn => "Processing...",
            GameState.MonsterTurn => "Enemy Turn...",
            GameState.Victory => "VICTORY!",
            GameState.GameOver => "GAME OVER",
            _ => "..."
        };
    }

    private void OnEndTurnClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnEndTurnClicked();
    }

    private void OnStartClicked()
    {
        if (startPanel != null)
            startPanel.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    private void OnRestartClicked()
    {
        if (gameEndPanel != null)
            gameEndPanel.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    private void OnGameStateChanged(GameState newState)
    {
        UpdateGameStateUI();
        if (startPanel != null && newState != GameState.NotStarted)
            startPanel.SetActive(false);
        bool isPlayerTurn = (newState == GameState.PlayerTurn);
        if (endTurnButton != null)
            endTurnButton.interactable = isPlayerTurn;
    }

    private void OnMonsterDamaged(int currentHealth, int maxHealth)
    {
        UpdateMonsterHealthUI();
    }

    private void OnMonsterStageChanged(int newStage)
    {
        UpdateMonsterHealthUI();
    }

    private void OnHandUpdated(int cardCount)
    {
        UpdateHandUI();
    }

    private void OnTurnStarted(int turnNumber)
    {
        UpdateTurnUI();
        UpdatePlayerHealthUI();
    }

    private void OnRoundChanged(int roundNumber)
    {
        UpdateTurnUI();
    }

    public void ShowGameEndScreen(bool victory)
    {
        if (gameEndPanel != null)
        {
            gameEndPanel.SetActive(true);
            Image panelBg = gameEndPanel.GetComponent<Image>();
            if (panelBg != null)
                panelBg.color = victory
                    ? new Color(0.12f, 0.28f, 0.12f, 0.96f)
                    : new Color(0.28f, 0.12f, 0.12f, 0.96f);
        }
        if (gameEndText != null)
            gameEndText.text = victory ? "VICTORY!" : "GAME OVER";
        if (gameEndSubtext != null)
            gameEndSubtext.text = victory ? "All masks destroyed!" : "You were defeated!";
        if (restartButton == null && gameEndPanel != null)
            restartButton = gameEndPanel.GetComponentInChildren<Button>(true);
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
            restartButton.interactable = true;
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
        }
    }

    private void Update()
    {
        UpdatePlayerHealthUI();
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.NotStarted)
        {
            if (startPanel == null)
            {
                Transform p = transform.Find("StartPanel");
                if (p != null)
                {
                    startPanel = p.gameObject;
                    startButton = startPanel.GetComponentInChildren<Button>(true);
                    if (startButton != null)
                    {
                        startButton.onClick.RemoveAllListeners();
                        startButton.onClick.AddListener(OnStartClicked);
                    }
                }
            }
            if (startPanel != null)
                startPanel.SetActive(true);
        }
    }
}
