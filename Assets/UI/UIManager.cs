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
    [SerializeField] private Button introductionButton;

    [Header("Introduction")]
    [TextArea(4, 14)]
    [SerializeField] private string introductionText = "You come to your senses in an unknown, desolate place—face to face with a masked entity. You demand to know who they are and ask them to remove the mask; they refuse with harsh, condescending scorn.\n\nIn combat, memories return: you once fought an all-powerful being whose strength did not belong to them but to a mysterious item they possessed. In time, you uncover the truth—the masked figure is that same foe who defeated you. That is why you awoke here.\n\nYou seek the bathrobe to claim that power for yourself, but the entity deflects you with a devastating spell...\n\nThis is a card game with three card types—Damage, Defense and Ability. Hover on cards to learn more. Combine them wisely for bonuses. Can you unravel who is behind the mask?";

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

    [Header("Final Boss Cutscene")]
    [SerializeField] private float screenFadeToBlackDuration = 1.2f;

    [Header("End Credits")]
    [SerializeField] private float creditsDisplayDuration = 5f;
    [SerializeField] private float creditsSlideDuration = 1.2f;
    [SerializeField] private float creditsLineSpacing = 48f;
    [SerializeField] private float creditsStartOffsetY = 400f;

    /// <summary>Duration used for fade-to-black in the final cutscene (configurable in Inspector).</summary>
    public float ScreenFadeToBlackDuration => screenFadeToBlackDuration;

    private GameObject blackOverlayPanel;
    private Image blackOverlayImage;
    private GameObject finalBossImagePanel;
    private Image finalBossImage;
    private RawImage finalBossRawImage;
    private GameObject creditsPanel;
    private RectTransform creditsContainerRect;
    private Coroutine creditsRedirectCoroutine;
    private GameObject introductionPanel;

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
        if (introductionButton == null)
            introductionButton = FindButtonByName("IntroductionButton");

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

        EnsureIntroductionButton();
    }

    /// <summary>
    /// Creates the Introduction button on the start panel if missing.
    /// </summary>
    private void EnsureIntroductionButton()
    {
        if (introductionButton != null || startPanel == null) return;

        GameObject btnObj = new GameObject("IntroductionButton");
        btnObj.transform.SetParent(startPanel.transform, false);
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0f, -220f);
        btnRect.sizeDelta = new Vector2(280f, 56f);

        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.25f, 0.2f, 0.35f, 0.95f);
        introductionButton = btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Introduction";
        tmp.fontSize = 28;
        tmp.color = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
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
        if (introductionButton != null)
        {
            introductionButton.onClick.RemoveAllListeners();
            introductionButton.onClick.AddListener(OnIntroductionClicked);
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

    private void OnIntroductionClicked()
    {
        EnsureIntroductionPanel();
        if (introductionPanel != null)
        {
            introductionPanel.SetActive(true);
            introductionPanel.transform.SetAsLastSibling();
        }
    }

    private void OnIntroductionCloseClicked()
    {
        if (introductionPanel != null)
            introductionPanel.SetActive(false);
    }

    /// <summary>
    /// Ensures the introduction panel exists (created at runtime if needed).
    /// Shows the game explanation text and a Close button.
    /// </summary>
    private void EnsureIntroductionPanel()
    {
        if (introductionPanel != null) return;

        introductionPanel = new GameObject("IntroductionPanel");
        introductionPanel.transform.SetParent(transform, false);

        RectTransform panelRect = introductionPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image bg = introductionPanel.AddComponent<Image>();
        bg.color = new Color(0.04f, 0.04f, 0.1f, 0.94f);
        bg.raycastTarget = true;

        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(introductionPanel.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.1f, 0.15f);
        contentRect.anchorMax = new Vector2(0.9f, 0.85f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("IntroductionText");
        textObj.transform.SetParent(contentObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0.2f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(24f, 24f);
        textRect.offsetMax = new Vector2(-24f, -80f);

        TextMeshProUGUI introText = textObj.AddComponent<TextMeshProUGUI>();
        introText.text = string.IsNullOrEmpty(introductionText)
            ? "You come to your senses in an unknown, desolate place—face to face with a masked entity. You demand to know who they are and ask them to remove the mask; they refuse with harsh, condescending scorn. In combat, memories return: you once fought an all-powerful being whose strength did not belong to them but to a mysterious item they possessed. In time, you uncover the truth—the masked figure is that same foe who defeated you. That is why you awoke here. You seek the bathrobe to claim that power for yourself, but the entity deflects you with a devastating spell... This is a card game with three card types—Damage, Defense and Ability. Hover on cards to learn more. Combine them wisely for bonuses."
            : introductionText;
        introText.fontSize = 32;
        introText.color = new Color(0.95f, 0.93f, 0.9f, 1f);
        introText.alignment = TMPro.TextAlignmentOptions.TopLeft;
        introText.enableWordWrapping = true;
        introText.raycastTarget = false;

        GameObject closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(contentObj.transform, false);
        RectTransform closeBtnRect = closeBtnObj.AddComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(0.5f, 0f);
        closeBtnRect.anchorMax = new Vector2(0.5f, 0f);
        closeBtnRect.pivot = new Vector2(0.5f, 0f);
        closeBtnRect.anchoredPosition = new Vector2(0f, 20f);
        closeBtnRect.sizeDelta = new Vector2(200f, 52f);

        Image closeBtnImg = closeBtnObj.AddComponent<Image>();
        closeBtnImg.color = new Color(0.3f, 0.25f, 0.4f, 0.98f);
        Button closeBtn = closeBtnObj.AddComponent<Button>();
        closeBtn.onClick.AddListener(OnIntroductionCloseClicked);

        GameObject closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        TextMeshProUGUI closeTmp = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeTmp.text = "Close";
        closeTmp.fontSize = 26;
        closeTmp.color = Color.white;
        closeTmp.alignment = TMPro.TextAlignmentOptions.Center;
        closeTmp.raycastTarget = false;

        introductionPanel.SetActive(false);
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
        if (creditsRedirectCoroutine != null)
        {
            StopCoroutine(creditsRedirectCoroutine);
            creditsRedirectCoroutine = null;
        }
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
        if (gameEndPanel != null)
            gameEndPanel.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    /// <summary>
    /// Ensures the fullscreen black overlay exists (created at runtime if needed).
    /// </summary>
    private void EnsureBlackOverlay()
    {
        if (blackOverlayPanel != null) return;

        blackOverlayPanel = new GameObject("BlackOverlay");
        blackOverlayPanel.transform.SetParent(transform, false);

        RectTransform rect = blackOverlayPanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.SetAsLastSibling();

        blackOverlayImage = blackOverlayPanel.AddComponent<Image>();
        blackOverlayImage.color = new Color(0f, 0f, 0f, 0f);
        blackOverlayImage.raycastTarget = false;

        blackOverlayPanel.SetActive(false);
    }

    /// <summary>
    /// Fades the screen to black over the given duration. Creates overlay if needed.
    /// </summary>
    /// <param name="duration">Fade duration in seconds.</param>
    public IEnumerator FadeToBlack(float duration)
    {
        EnsureBlackOverlay();
        blackOverlayPanel.SetActive(true);
        blackOverlayPanel.transform.SetAsLastSibling();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            blackOverlayImage.color = new Color(0f, 0f, 0f, t);
            yield return null;
        }
        blackOverlayImage.color = new Color(0f, 0f, 0f, 1f);
    }

    /// <summary>
    /// Ensures the Final Boss unmasked image panel exists (created at runtime if needed).
    /// Tries Sprite first, then Texture2D so it works regardless of import settings.
    /// </summary>
    private void EnsureFinalBossImagePanel()
    {
        if (finalBossImagePanel != null) return;

        Sprite sprite = Resources.Load<Sprite>("FinalBoss/FinalBossUnmasked");
        Texture2D texture = Resources.Load<Texture2D>("FinalBoss/FinalBossUnmasked");

        if (sprite == null && texture == null)
        {
            Debug.LogWarning("UIManager: FinalBoss/FinalBossUnmasked not found in Resources (tried Sprite and Texture2D).");
            return;
        }

        finalBossImagePanel = new GameObject("FinalBossImagePanel");
        finalBossImagePanel.transform.SetParent(transform, false);

        RectTransform rect = finalBossImagePanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        if (sprite != null)
        {
            finalBossImage = finalBossImagePanel.AddComponent<Image>();
            finalBossImage.sprite = sprite;
            finalBossImage.preserveAspect = true;
            finalBossImage.color = Color.white;
            finalBossImage.raycastTarget = false;
        }
        else
        {
            finalBossRawImage = finalBossImagePanel.AddComponent<RawImage>();
            finalBossRawImage.texture = texture;
            finalBossRawImage.color = Color.white;
            finalBossRawImage.raycastTarget = false;
            finalBossRawImage.uvRect = new Rect(0, 0, 1, 1);
        }

        finalBossImagePanel.SetActive(false);
    }

    /// <summary>
    /// Shows or hides the Final Boss unmasked image (on top of black screen).
    /// Brings panel to front when showing so it appears above the black overlay.
    /// </summary>
    public void ShowFinalBossImage(bool show)
    {
        EnsureFinalBossImagePanel();
        if (finalBossImagePanel != null)
        {
            finalBossImagePanel.SetActive(show);
            if (show)
                finalBossImagePanel.transform.SetAsLastSibling();
        }
    }

    /// <summary>
    /// Ensures the end credits panel exists (created at runtime if needed).
    /// Credits are separate lines with spacing, in a container that animates from top to center.
    /// </summary>
    private void EnsureCreditsPanel()
    {
        if (creditsPanel != null) return;

        creditsPanel = new GameObject("CreditsPanel");
        creditsPanel.transform.SetParent(transform, false);

        RectTransform panelRect = creditsPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image bg = creditsPanel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.12f, 0.92f);
        bg.raycastTarget = false;

        GameObject containerObj = new GameObject("CreditsContainer");
        containerObj.transform.SetParent(creditsPanel.transform, false);
        creditsContainerRect = containerObj.AddComponent<RectTransform>();
        creditsContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
        creditsContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
        creditsContainerRect.pivot = new Vector2(0.5f, 0.5f);
        creditsContainerRect.sizeDelta = new Vector2(700f, 220f);
        creditsContainerRect.anchoredPosition = new Vector2(0f, creditsStartOffsetY);

        string[] lines = new[]
        {
            "Developers: Branimir and Sofia",
            "Designers: Ivan and Vladislav",
            "Music: Ivan"
        };
        float currentY = 0f;
        for (int i = 0; i < lines.Length; i++)
        {
            GameObject lineObj = new GameObject("CreditsLine_" + i);
            lineObj.transform.SetParent(creditsContainerRect, false);
            RectTransform lineRect = lineObj.AddComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0.5f, 1f);
            lineRect.anchorMax = new Vector2(0.5f, 1f);
            lineRect.pivot = new Vector2(0.5f, 1f);
            lineRect.anchoredPosition = new Vector2(0f, currentY);
            lineRect.sizeDelta = new Vector2(650f, 52f);

            TextMeshProUGUI lineText = lineObj.AddComponent<TextMeshProUGUI>();
            lineText.text = lines[i];
            lineText.fontSize = 42;
            lineText.color = Color.white;
            lineText.alignment = TMPro.TextAlignmentOptions.Center;
            lineText.enableWordWrapping = true;
            lineText.raycastTarget = false;

            currentY -= (52f + creditsLineSpacing);
        }

        creditsPanel.SetActive(false);
    }

    /// <summary>
    /// Shows the end credits panel, runs the slide-in animation, then starts the redirect timer.
    /// </summary>
    private void ShowCreditsAndScheduleRedirect()
    {
        EnsureCreditsPanel();
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
            creditsPanel.transform.SetAsLastSibling();
            if (creditsContainerRect != null)
                creditsContainerRect.anchoredPosition = new Vector2(0f, creditsStartOffsetY);
        }
        if (creditsRedirectCoroutine != null)
            StopCoroutine(creditsRedirectCoroutine);
        creditsRedirectCoroutine = StartCoroutine(CreditsAnimationAndRedirectCoroutine());
    }

    private IEnumerator CreditsAnimationAndRedirectCoroutine()
    {
        if (creditsContainerRect != null)
        {
            float elapsed = 0f;
            float startY = creditsStartOffsetY;
            Vector2 startPos = new Vector2(0f, startY);
            Vector2 endPos = new Vector2(0f, 0f);

            while (elapsed < creditsSlideDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / creditsSlideDuration);
                float eased = 1f - (1f - t) * (1f - t);
                creditsContainerRect.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
                yield return null;
            }
            creditsContainerRect.anchoredPosition = endPos;
        }

        yield return new WaitForSeconds(creditsDisplayDuration);
        creditsRedirectCoroutine = null;
        if (creditsPanel != null)
            creditsPanel.SetActive(false);
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

        if (victory)
        {
            ShowCreditsAndScheduleRedirect();
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
