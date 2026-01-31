using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GameSetup : MonoBehaviour
{
    [Header("Setup Settings")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private bool createUICanvas = true;

    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupGame();
        }
    }

    [ContextMenu("Setup Game")]
    public void SetupGame()
    {
        Debug.Log("=== Setting up game scene ===");
        
        CreateGameManager();
        CreatePlayer();
        CreateMonster();
        CreatePlayerBoard();
        CreateEnemyBoard();
        CreateDeck();
        CreateHand();
        CreateEnemyAI();
        
        if (createUICanvas)
        {
            CreateUI();
        }
        
        CreateCamera();
        CreateLighting();
        
        Debug.Log("=== Game setup complete! ===");
    }

    private void CreateGameManager()
    {
        if (FindObjectOfType<GameManager>() != null) return;
        
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();
        gmObj.AddComponent<TurnManager>();
        
        Debug.Log("Created: GameManager");
    }

    private void CreatePlayer()
    {
        if (FindObjectOfType<Player>() != null) return;
        
        GameObject playerObj = new GameObject("Player");
        playerObj.AddComponent<Player>();
        playerObj.transform.position = new Vector3(-5f, 0, -5f);
        
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "PlayerVisual";
        visual.transform.SetParent(playerObj.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.2f, 0.5f, 0.9f);
        }
        
        Debug.Log("Created: Player");
    }

    private void CreateMonster()
    {
        if (FindObjectOfType<Monster>() != null) return;
        
        GameObject monsterObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        monsterObj.name = "Monster";
        monsterObj.transform.position = new Vector3(0, 0.75f, 6f);
        monsterObj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        
        monsterObj.AddComponent<Monster>();
        monsterObj.AddComponent<MonsterVisual>();
        
        Debug.Log("Created: Monster");
    }

    private void CreatePlayerBoard()
    {
        BoardManager[] boards = FindObjectsOfType<BoardManager>();
        foreach (var b in boards)
        {
            if (b.IsPlayerBoard) return;
        }
        
        GameObject boardObj = new GameObject("PlayerBoard");
        boardObj.transform.position = new Vector3(0, 0, -4f);
        
        BoardManager board = boardObj.AddComponent<BoardManager>();
        // isPlayerBoard defaults to true
        
        // Ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "PlayerGround";
        ground.transform.position = new Vector3(0, 0, -2.5f);
        ground.transform.localScale = new Vector3(0.6f, 1, 0.5f);
        ground.GetComponent<Renderer>().material.color = new Color(0.2f, 0.25f, 0.3f);
        
        Debug.Log("Created: Player Board");
    }

    private void CreateEnemyBoard()
    {
        // BoardManager[] boards = FindObjectsOfType<BoardManager>();
        // foreach (var b in boards)
        // {
        //     if (!b.IsPlayerBoard) return;
        // }
        
        // GameObject boardObj = new GameObject("EnemyBoard");
        // boardObj.transform.position = new Vector3(0, 0, 4f);
        
        // BoardManager board = boardObj.AddComponent<BoardManager>();
        // // Need to set isPlayerBoard to false via reflection
        // var field = typeof(BoardManager).GetField("isPlayerBoard", 
        //     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        // if (field != null)
        // {
        //     field.SetValue(board, false);
        // }
        
        // // Ground
        // GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        // ground.name = "EnemyGround";
        // ground.transform.position = new Vector3(0, 0, 2.5f);
        // ground.transform.localScale = new Vector3(0.6f, 1, 0.5f);
        // ground.GetComponent<Renderer>().material.color = new Color(0.3f, 0.2f, 0.2f);
        
        // Debug.Log("Created: Enemy Board");
    }

    private void CreateDeck()
    {
        if (FindObjectOfType<Deck>() != null) return;

        GameObject deckObj = new GameObject("Deck");
        deckObj.transform.position = new Vector3(0f, 0f, 0f);
        deckObj.AddComponent<Deck>();

        Debug.Log("Created: Deck (shared by player and enemy)");
    }

    private void CreateHand()
    {
        if (FindObjectOfType<Hand>() != null) return;
        
        GameObject handObj = new GameObject("Hand");
        handObj.transform.position = new Vector3(0, 0, -6f);
        handObj.AddComponent<Hand>();
        
        Debug.Log("Created: Hand");
    }

    private void CreateEnemyAI()
    {
        if (FindObjectOfType<EnemyAI>() != null) return;
        
        GameObject aiObj = new GameObject("EnemyAI");
        aiObj.AddComponent<EnemyAI>();
        
        Debug.Log("Created: EnemyAI");
    }

    private void CreateUI()
    {
        EnsureEventSystem();

        UIManager existingUI = FindObjectOfType<UIManager>();
        if (existingUI != null)
        {
            // UI already exists, but ensure DialoguePanel is created
            EnsureDialoguePanel(existingUI.transform);
            return;
        }

        GameObject canvasObj = new GameObject("UICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.AddComponent<UIManager>();
        
        GameObject keybindsObj = CreateText(canvasObj.transform, "KeybindsText",
            "E - place card\nQ - withdraw card\nSpace - end turn\n1-5 - select card\nR - restart",
            new Vector2(800, -180), 18);
        if (keybindsObj != null)
            keybindsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 100);

        CreateButton(canvasObj.transform, "EndTurnButton", "END TURN",
            new Vector2(800, -300), new Vector2(200, 80));

        CreateText(canvasObj.transform, "TurnText", "Turn: 1", new Vector2(-800, 480), 28);
        CreateText(canvasObj.transform, "RoundText", "Round: 1", new Vector2(-800, 440), 24);
        CreateText(canvasObj.transform, "GameStateText", "Your Turn", new Vector2(0, 480), 32);
        CreateText(canvasObj.transform, "PlayerHealthText", "Player HP: 100/100", new Vector2(-800, -450), 24);
        CreateText(canvasObj.transform, "MonsterHealthText", "Monster HP: 100/100", new Vector2(0, 400), 24);
        CreateText(canvasObj.transform, "MonsterStageText", "Mask 1/3", new Vector2(0, 360), 20);
        CreateText(canvasObj.transform, "HandCountText", "Hand: 6/10", new Vector2(800, -450), 20);
        CreateText(canvasObj.transform, "DeckCountText", "Deck: 20", new Vector2(800, -480), 20);

        GameObject comboObj = CreateText(canvasObj.transform, "ComboStatusText", "",
            new Vector2(20, 0), 18);
        if (comboObj != null)
        {
            RectTransform comboRect = comboObj.GetComponent<RectTransform>();
            if (comboRect != null)
            {
                comboRect.anchorMin = new Vector2(0, 0.5f);
                comboRect.anchorMax = new Vector2(0, 0.5f);
                comboRect.pivot = new Vector2(0, 0.5f);
                comboRect.anchoredPosition = new Vector2(20, 0);
                comboRect.sizeDelta = new Vector2(300, 240);
            }
            var tmp = comboObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.alignment = TMPro.TextAlignmentOptions.TopLeft;
                tmp.enableWordWrapping = true;
            }
        }

        CreateStartPanel(canvasObj.transform);
        CreateGameEndPanel(canvasObj.transform);
        CreateDialoguePanel(canvasObj.transform);

        Debug.Log("Created: UI Canvas");
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
            return;
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<StandaloneInputModule>();
        Debug.Log("Created: EventSystem (required for UI button clicks)");
    }

    private void EnsureDialoguePanel(Transform canvasTransform)
    {
        // Check if DialoguePanel already exists
        Transform existingPanel = canvasTransform.Find("DialoguePanel");
        if (existingPanel != null)
        {
            Debug.Log("DialoguePanel already exists");
            // Ensure DialogueManager exists on canvas
            if (canvasTransform.GetComponent<DialogueManager>() == null)
            {
                DialogueManager dm = canvasTransform.gameObject.AddComponent<DialogueManager>();
                // Find and set references
                TextMeshProUGUI nameText = existingPanel.Find("SpeakerNameText")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI dialogueText = existingPanel.Find("DialogueText")?.GetComponent<TextMeshProUGUI>();
                Button nextButton = existingPanel.Find("DialogueNextButton")?.GetComponent<Button>();
                dm.SetReferences(existingPanel.gameObject, nameText, dialogueText, nextButton);
            }
            return;
        }

        // Create DialoguePanel
        CreateDialoguePanel(canvasTransform);
    }

    private void CreateStartPanel(Transform parent)
    {
        GameObject panel = new GameObject("StartPanel");
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.15f, 0.98f);

        GameObject titleObj = CreateText(panel.transform, "StartTitle", "CARD GAME", new Vector2(0, 120), 64);
        if (titleObj != null)
            titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 80);

        CreateText(panel.transform, "StartSubtitle", "Defeat the masks", new Vector2(0, 20), 28);

        GameObject playBtn = CreateButton(panel.transform, "StartButton", "PLAY", new Vector2(0, -120), new Vector2(280, 80));
        if (playBtn != null)
        {
            Image btnImg = playBtn.GetComponent<Image>();
            if (btnImg != null)
                btnImg.color = new Color(0.2f, 0.5f, 0.25f);
        }

        panel.SetActive(true);
    }

    private GameObject CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.4f, 0.6f);
        
        buttonObj.AddComponent<Button>();
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 28;
        tmp.color = Color.white;
        
        return buttonObj;
    }

    private GameObject CreateText(Transform parent, string name, string text, Vector2 position, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(400, 50);
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        
        return textObj;
    }

    private void CreateGameEndPanel(Transform parent)
    {
        GameObject panel = new GameObject("GameEndPanel");
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.9f);

        GameObject endTextObj = CreateText(panel.transform, "GameEndText", "GAME OVER", new Vector2(0, 80), 56);
        if (endTextObj != null)
            endTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 100);
        CreateText(panel.transform, "GameEndSubtext", "You were defeated!", new Vector2(0, 0), 28);
        CreateButton(panel.transform, "RestartButton", "PLAY AGAIN", new Vector2(0, -120), new Vector2(280, 70));

        panel.SetActive(false);
    }

    private void CreateDialoguePanel(Transform parent)
    {
        // Main dialogue panel - positioned at bottom with 10% margin
        GameObject panel = new GameObject("DialoguePanel");
        panel.transform.SetParent(parent, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        // Anchor to bottom, with 10% margin from bottom
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.35f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Semi-transparent dark background
        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

        // Add border effect
        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.3f, 0.3f, 0.4f, 1f);
        outline.effectDistance = new Vector2(3, 3);

        // Speaker name text - positioned at top-left of panel
        GameObject nameObj = new GameObject("SpeakerNameText");
        nameObj.transform.SetParent(panel.transform, false);

        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(0, 1);
        nameRect.pivot = new Vector2(0, 1);
        nameRect.anchoredPosition = new Vector2(20, 30);
        nameRect.sizeDelta = new Vector2(400, 50);

        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "Speaker";
        nameText.fontSize = 28;
        nameText.fontStyle = TMPro.FontStyles.Bold;
        nameText.color = new Color(0.4f, 0.7f, 1f);
        nameText.alignment = TMPro.TextAlignmentOptions.Left;

        // Name background for better visibility
        GameObject nameBgObj = new GameObject("SpeakerNameBg");
        nameBgObj.transform.SetParent(panel.transform, false);
        nameBgObj.transform.SetSiblingIndex(0); // Put behind name text

        RectTransform nameBgRect = nameBgObj.AddComponent<RectTransform>();
        nameBgRect.anchorMin = new Vector2(0, 1);
        nameBgRect.anchorMax = new Vector2(0, 1);
        nameBgRect.pivot = new Vector2(0, 1);
        nameBgRect.anchoredPosition = new Vector2(10, 40);
        nameBgRect.sizeDelta = new Vector2(250, 45);

        Image nameBgImage = nameBgObj.AddComponent<Image>();
        nameBgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.98f);

        // Dialogue text - main content area
        GameObject dialogueObj = new GameObject("DialogueText");
        dialogueObj.transform.SetParent(panel.transform, false);

        RectTransform dialogueRect = dialogueObj.AddComponent<RectTransform>();
        dialogueRect.anchorMin = new Vector2(0, 0);
        dialogueRect.anchorMax = new Vector2(1, 1);
        dialogueRect.offsetMin = new Vector2(30, 60);
        dialogueRect.offsetMax = new Vector2(-30, -20);

        TextMeshProUGUI dialogueText = dialogueObj.AddComponent<TextMeshProUGUI>();
        dialogueText.text = "";
        dialogueText.fontSize = 26;
        dialogueText.color = Color.white;
        dialogueText.alignment = TMPro.TextAlignmentOptions.TopLeft;
        dialogueText.enableWordWrapping = true;

        // Next button - positioned at bottom-right of panel
        GameObject nextBtn = CreateButton(panel.transform, "DialogueNextButton", "Next",
            new Vector2(0, 0), new Vector2(140, 50));

        if (nextBtn != null)
        {
            RectTransform btnRect = nextBtn.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1, 0);
            btnRect.anchorMax = new Vector2(1, 0);
            btnRect.pivot = new Vector2(1, 0);
            btnRect.anchoredPosition = new Vector2(-20, 15);

            Image btnImg = nextBtn.GetComponent<Image>();
            if (btnImg != null)
                btnImg.color = new Color(0.25f, 0.45f, 0.65f);
        }

        // Add DialogueManager component to the CANVAS (parent) so it can be found when panel is inactive
        DialogueManager dialogueManager = parent.GetComponent<DialogueManager>();
        if (dialogueManager == null)
            dialogueManager = parent.gameObject.AddComponent<DialogueManager>();

        // Set references
        Button nextButton = nextBtn != null ? nextBtn.GetComponent<Button>() : null;
        dialogueManager.SetReferences(panel, nameText, dialogueText, nextButton);

        Debug.Log($"DialogueManager created on Canvas. Panel: {panel.name}");

        panel.SetActive(false);
        Debug.Log("Created: Dialogue Panel");
    }

    private void CreateCamera()
    {
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
        }
        
        mainCamera.transform.position = new Vector3(0, 12, -8);
        mainCamera.transform.rotation = Quaternion.Euler(50, 0, 0);
        mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        
        Debug.Log("Configured: Camera");
    }

    private void CreateLighting()
    {
        Light directionalLight = null;
        Light[] lights = FindObjectsOfType<Light>();
        
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                directionalLight = light;
                break;
            }
        }
        
        if (directionalLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            directionalLight = lightObj.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
        }
        
        directionalLight.transform.rotation = Quaternion.Euler(50, -30, 0);
        directionalLight.intensity = 1.2f;
        
        Debug.Log("Configured: Lighting");
    }
}
