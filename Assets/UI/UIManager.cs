using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

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

    [Header("Combo Status (Row 0)")]
    [SerializeField] private TextMeshProUGUI comboStatusText;

    [Header("Start Screen")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private Button startButton;

    [Header("Game End Screen")]
    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TextMeshProUGUI gameEndText;
    [SerializeField] private TextMeshProUGUI gameEndSubtext;

    [Header("Dialogue System")]
    [SerializeField] private DialogueManager dialogueManager;

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Monster monster;
    [SerializeField] private Hand hand;
    [SerializeField] private Deck deck;
    [SerializeField] private TurnManager turnManager;

    public static UIManager Instance;

    [Header("Simple Preview")]
    [SerializeField] private GameObject previewPanel; // Сама панель
    [SerializeField] private RawImage previewImage;   // Компонент RawImage на этой панели

    private void Awake()
    {
        Instance = this;
        if (previewPanel != null)
        {
            previewImage.color = new Color(0, 0, 0, 0);
        }
        // ... остальной твой Awake
    }

    public void ShowPreview(Texture cardTexture)
    {
        if (previewPanel == null || previewImage == null || cardTexture == null) return;
        previewImage.texture = cardTexture;
        previewImage.color = new Color(1, 1, 1, 1);
    }

    public void HidePreview()
    {
        if (previewPanel != null)
        {
            previewImage.color = new Color(0, 0, 0, 0);
        }
    }

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
        if (comboStatusText == null)
        {
            comboStatusText = FindTextByName("ComboStatusText");
            if (comboStatusText == null)
                comboStatusText = CreateComboStatusText();
        }

        if (dialogueManager == null)
        {
            dialogueManager = GetComponent<DialogueManager>();
            if (dialogueManager == null)
                dialogueManager = FindObjectOfType<DialogueManager>(true);
        }
    }

    private TextMeshProUGUI CreateComboStatusText()
    {
        GameObject go = new GameObject("ComboStatusText");
        go.transform.SetParent(transform, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.anchoredPosition = new Vector2(20, 0);
        rect.sizeDelta = new Vector2(300, 240);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = "Комбо (ред 0):\n—";
        tmp.fontSize = 18;
        tmp.color = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.TopLeft;
        tmp.enableWordWrapping = true;
        if (TMPro.TMP_Settings.defaultFontAsset != null)
            tmp.font = TMPro.TMP_Settings.defaultFontAsset;
        return tmp;
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
        UpdateComboStatusText();
    }

    private void UpdateComboStatusText()
    {
        if (comboStatusText == null)
        {
            comboStatusText = FindTextByName("ComboStatusText");
            if (comboStatusText == null)
                comboStatusText = CreateComboStatusText();
        }
        if (GameManager.Instance == null || GameManager.Instance.PlayerBoard == null ||
            GameManager.Instance.EnemyBoard == null)
        {
            comboStatusText.text = "Комбо (ред 0):\n3 червени → +100% дмг\n3 сини → -50% дмг\n3 зелени → бонус x2";
            comboStatusText.gameObject.SetActive(true);
            return;
        }
        var lines = new System.Collections.Generic.List<string>();
        BoardManager playerBoard = GameManager.Instance.PlayerBoard;
        BoardManager enemyBoard = GameManager.Instance.EnemyBoard;

        if (playerBoard.HasRow0ColorCombo("red"))
            lines.Add("3 червени на ред\n+100% демедж скор (ти)");
        if (enemyBoard.HasRow0ColorCombo("red"))
            lines.Add("3 червени на ред\n+100% демедж скор (враг)");
        if (playerBoard.HasRow0ColorCombo("blue"))
            lines.Add("3 сини на ред\n-50% получаван демедж (ти)");
        if (enemyBoard.HasRow0ColorCombo("blue"))
            lines.Add("3 сини на ред\n-50% получаван демедж (враг)");
        if (playerBoard.HasRow0ColorCombo("green"))
            lines.Add("3 зелени на ред\nбонус зелени x2");

        string body = lines.Count > 0
            ? string.Join("\n\n", lines)
            : "3 червени → +100% дмг\n3 сини → -50% дмг\n3 зелени → бонус x2";
        comboStatusText.text = "Комбо (ред 0):\n" + body;
        comboStatusText.gameObject.SetActive(true);
    }

    private void UpdateGameStateUI()
    {
        if (gameStateText == null) return;

        if (GameManager.Instance == null)
        {
            gameStateText.text = "Waiting...";
            return;
        }

        switch (GameManager.Instance.CurrentState)
        {
            case GameState.PlayerTurn:
                int placed = turnManager != null ? turnManager.PlacementsThisTurn : 0;
                int maxPlace = turnManager != null ? turnManager.MaxPlacementsPerTurn : 3;
                gameStateText.text = $"YOUR TURN — {placed}/{3} cards";
                break;
            case GameState.ProcessingTurn:
                gameStateText.text = "Resolving...";
                break;
            case GameState.MonsterTurn:
                gameStateText.text = "Enemy turn";
                break;
            case GameState.Victory:
                gameStateText.text = "VICTORY!";
                break;
            case GameState.GameOver:
                gameStateText.text = "GAME OVER";
                break;
            default:
                gameStateText.text = "...";
                break;
        }
    }

    private void OnEndTurnClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnEndTurnClicked();
    }

    private void OnStartClicked()
    {
        StartCoroutine(ShowDialogueAfterPlayLoad());
    }

    /// <summary>
    /// Hides start panel, starts the game scene, then shows the dialogue on top.
    /// </summary>
    private IEnumerator ShowDialogueAfterPlayLoad()
    {
        if (startPanel != null)
            startPanel.SetActive(false);

        // Start the game first so the scene is visible
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
            UpdateAllUI();
        }

        // Wait a frame to let the game scene initialize
        yield return null;

        if (dialogueManager == null)
        {
            dialogueManager = GetComponent<DialogueManager>();
            if (dialogueManager == null)
                dialogueManager = FindObjectOfType<DialogueManager>(true);
            if (dialogueManager == null)
                dialogueManager = DialogueManager.EnsureOnCanvas(transform);
        }

        if (dialogueManager == null)
        {
            Debug.LogWarning("UIManager: DialogueManager not found! Game started without dialogue.");
            yield break;
        }

        // Show dialogue on top of the game scene
        dialogueManager.OnDialogueCompleted += OnDialogueCompleted;
        dialogueManager.StartDialogue();
    }

    private void OnDialogueCompleted()
    {
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueCompleted -= OnDialogueCompleted;
        }

        if (startPanel != null)
            startPanel.SetActive(false);

        // Game is already started, just update UI
        UpdateAllUI();
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
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.PlayerTurn)
        {
            UpdateGameStateUI();
        }
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
        else if (GameManager.Instance != null &&
                 GameManager.Instance.CurrentState != GameState.NotStarted &&
                 GameManager.Instance.CurrentState != GameState.Victory &&
                 GameManager.Instance.CurrentState != GameState.GameOver)
        {
            UpdateComboStatusText();
        }
    }
}
