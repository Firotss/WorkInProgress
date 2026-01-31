using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneSetupEditor : Editor
{
    [MenuItem("Tools/Setup Game Scene")]
    public static void SetupGameScene()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("Setup Game Scene cannot run during play mode. Stop Play first.");
            return;
        }

        Debug.Log("=== Setting up Game Scene ===");

        ClearScene();

        CreateGameController();
        CreateGameManager();
        CreatePlayer();
        CreateMonster();
        CreatePlayerBoard();
        CreateEnemyBoard();
        CreateDeck();
        CreateHand();
        CreateEnemyAI();
        CreateUI();
        EnsureEventSystem();
        SetupCamera();
        SetupLighting();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("=== Game Scene Setup Complete! Press Play to start. ===");
    }

    private static void ClearScene()
    {
        string[] objectsToDestroy = { 
            "GameController", "GameManager", "Player", "Monster", 
            "PlayerBoard", "EnemyBoard", "Board", "PlayerDeck", "EnemyDeck",
            "Deck", "Hand", "EnemyAI", "UICanvas", "Ground" 
        };
        
        foreach (string name in objectsToDestroy)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
    }

    private static void CreateGameController()
    {
        GameObject controller = new GameObject("GameController");
        Initiator initiator = controller.AddComponent<Initiator>();
        controller.AddComponent<InputHandler>();
        
        SerializedObject so = new SerializedObject(initiator);
        so.FindProperty("useAutoSetup").boolValue = false;
        so.ApplyModifiedProperties();
        
        Debug.Log("Created: GameController");
    }

    private static void CreateGameManager()
    {
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();
        gmObj.AddComponent<TurnManager>();
        
        Debug.Log("Created: GameManager");
    }

    private static void CreatePlayer()
    {
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
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 0.5f, 0.9f);
            renderer.material = mat;
        }
        
        Debug.Log("Created: Player");
    }

    private static void CreateMonster()
    {
        GameObject monsterObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        monsterObj.name = "Monster";
        monsterObj.transform.position = new Vector3(0, 0.75f, 6f);
        monsterObj.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        
        monsterObj.AddComponent<Monster>();
        monsterObj.AddComponent<MonsterVisual>();
        
        Debug.Log("Created: Monster");
    }

    private static void CreatePlayerBoard()
    {
        GameObject boardObj = new GameObject("PlayerBoard");
        boardObj.transform.position = new Vector3(0, 0, -4f);
        
        BoardManager board = boardObj.AddComponent<BoardManager>();
        
        SerializedObject so = new SerializedObject(board);
        so.FindProperty("isPlayerBoard").boolValue = true;
        so.ApplyModifiedProperties();

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "PlayerGround";
        ground.transform.position = new Vector3(0, 0, -2.5f);
        ground.transform.localScale = new Vector3(0.6f, 1, 0.5f);
        
        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        
        Debug.Log("Created: Player Board");
    }

    private static void CreateEnemyBoard()
    {
        GameObject boardObj = new GameObject("EnemyBoard");
        boardObj.transform.position = new Vector3(0, 0, 4f); // Row 0 here, rows go toward 0
        
        BoardManager board = boardObj.AddComponent<BoardManager>();
        
        SerializedObject so = new SerializedObject(board);
        so.FindProperty("isPlayerBoard").boolValue = false;
        so.ApplyModifiedProperties();

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "EnemyGround";
        ground.transform.position = new Vector3(0, 0, 2.5f);
        ground.transform.localScale = new Vector3(0.6f, 1, 0.5f);
        
        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
        
        Debug.Log("Created: Enemy Board");
    }

    private static void CreateDeck()
    {
        GameObject deckObj = new GameObject("Deck");
        deckObj.transform.position = new Vector3(0f, 0f, 0f);
        deckObj.AddComponent<Deck>();

        Debug.Log("Created: Deck (shared by player and enemy)");
    }

    private static void CreateHand()
    {
        GameObject handObj = new GameObject("Hand");
        handObj.transform.position = new Vector3(0, 0, -6f);
        Hand hand = handObj.AddComponent<Hand>();
        
        Debug.Log("Created: Hand");
    }

    private static void CreateEnemyAI()
    {
        GameObject aiObj = new GameObject("EnemyAI");
        aiObj.AddComponent<EnemyAI>();
        
        Debug.Log("Created: EnemyAI");
    }

    private static void CreateUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("UICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        canvasObj.AddComponent<UIManager>();
        
        GameObject keybindsObj = CreateText(canvasObj.transform, "KeybindsText",
            "E - place card\nQ - withdraw card\nSpace - end turn\n1-5 - select card\nR - restart",
            new Vector2(800, -180), 18);
        if (keybindsObj != null)
            keybindsObj.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 100);

        CreateButton(canvasObj.transform, "EndTurnButton", "END TURN",
            new Vector2(800, -300), new Vector2(200, 80));
        
        // Info texts
        CreateText(canvasObj.transform, "TurnText", "Turn: 1", new Vector2(-800, 480), 28);
        CreateText(canvasObj.transform, "RoundText", "Round: 1", new Vector2(-800, 440), 24);
        CreateText(canvasObj.transform, "GameStateText", "Your Turn - Place Cards!", new Vector2(0, 480), 32);
        
        // Player health - bottom left
        CreateText(canvasObj.transform, "PlayerHealthText", "Player HP: 100/100", new Vector2(-800, -450), 24);
        
        // Monster health - top center
        CreateText(canvasObj.transform, "MonsterHealthText", "Monster HP: 100/100", new Vector2(0, 400), 24);
        CreateText(canvasObj.transform, "MonsterStageText", "Mask 1/3", new Vector2(0, 360), 20);
        
        // Hand/Deck info - bottom right
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
            var tmp = comboObj.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.alignment = TMPro.TextAlignmentOptions.TopLeft;
                tmp.enableWordWrapping = true;
            }
        }

        CreateStartPanel(canvasObj.transform);
        CreateGameEndPanel(canvasObj.transform);

        Debug.Log("Created: UI Canvas");
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
            return;
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<StandaloneInputModule>();
    }

    private static void CreateStartPanel(Transform parent)
    {
        GameObject panel = new GameObject("StartPanel");
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        UnityEngine.UI.Image bg = panel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0.08f, 0.08f, 0.15f, 0.98f);

        GameObject titleObj = CreateText(panel.transform, "StartTitle", "CARD GAME", new Vector2(0, 120), 64);
        if (titleObj != null)
            titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 80);

        CreateText(panel.transform, "StartSubtitle", "Defeat the masks", new Vector2(0, 20), 28);

        GameObject playBtn = CreateButton(panel.transform, "StartButton", "PLAY", new Vector2(0, -120), new Vector2(280, 80));
        if (playBtn != null)
        {
            var btnImg = playBtn.GetComponent<UnityEngine.UI.Image>();
            if (btnImg != null)
                btnImg.color = new Color(0.2f, 0.5f, 0.25f);
        }

        panel.SetActive(true);
    }

    private static GameObject CreateButton(Transform parent, string name, string text, 
        Vector2 position, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        
        UnityEngine.UI.Image image = buttonObj.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.2f, 0.4f, 0.6f);
        
        UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();
        
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.7f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.5f);
        button.colors = colors;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TMPro.TextMeshProUGUI tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontSize = 28;
        tmp.color = Color.white;
        
        return buttonObj;
    }

    private static GameObject CreateText(Transform parent, string name, string text, 
        Vector2 position, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(400, 50);
        
        TMPro.TextMeshProUGUI tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        
        return textObj;
    }

    private static void CreateGameEndPanel(Transform parent)
    {
        GameObject panel = new GameObject("GameEndPanel");
        panel.transform.SetParent(parent, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        UnityEngine.UI.Image bg = panel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0, 0, 0, 0.9f);
        
        GameObject endTextObj = CreateText(panel.transform, "GameEndText", "GAME OVER", new Vector2(0, 80), 56);
        if (endTextObj != null)
            endTextObj.GetComponent<RectTransform>().sizeDelta = new Vector2(700, 100);
        CreateText(panel.transform, "GameEndSubtext", "You were defeated!", new Vector2(0, 0), 28);
        CreateButton(panel.transform, "RestartButton", "PLAY AGAIN", new Vector2(0, -120), new Vector2(280, 70));

        panel.SetActive(false);
    }

    private static void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            mainCamera = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }
        
        mainCamera.transform.position = new Vector3(0, 12, -8);
        mainCamera.transform.rotation = Quaternion.Euler(50, 0, 0);
        mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        
        Debug.Log("Configured: Camera");
    }

    private static void SetupLighting()
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
        directionalLight.color = Color.white;
        
        Debug.Log("Configured: Lighting");
    }
}
