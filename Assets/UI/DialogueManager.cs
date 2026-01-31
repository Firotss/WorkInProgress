using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI nextButtonText;

    [Header("Settings")]
    [SerializeField] private float typewriterSpeed = 0.03f;
    [SerializeField] private Color playerNameColor = new Color(0.4f, 0.7f, 1f);
    [SerializeField] private Color maskedEntityNameColor = new Color(0.9f, 0.3f, 0.3f);

    private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typewriterCoroutine;
    private string currentFullText = "";

    public event Action OnDialogueCompleted;

    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string text;
        public bool isPlayer;

        public DialogueLine(string speaker, string dialogueText, bool player)
        {
            speakerName = speaker;
            text = dialogueText;
            isPlayer = player;
        }
    }

    private void Awake()
    {
        InitializeDialogue();
    }

    private void InitializeDialogue()
    {
        dialogueLines.Clear();

        dialogueLines.Add(new DialogueLine("Player", "What...What happened where am I and who are you", true));
        dialogueLines.Add(new DialogueLine("Masked Entity", "You will never know who I am or why you are here", false));
        dialogueLines.Add(new DialogueLine("Player", "Stop hiding behind that mask and tell me who you are or I will see it for myself", true));
        dialogueLines.Add(new DialogueLine("Masked Entity", "I would like to see you try", false));
    }

    public void StartDialogue()
    {
        // Try to find references if not set
        if (dialoguePanel == null)
        {
            FindReferences();
        }

        if (dialoguePanel == null)
        {
            Debug.LogError("DialogueManager: dialoguePanel is null! Cannot start dialogue.");
            OnDialogueCompleted?.Invoke();
            return;
        }

        Debug.Log("DialogueManager: Starting dialogue cutscene");
        dialoguePanel.SetActive(true);
        dialoguePanel.transform.SetAsLastSibling();
        currentLineIndex = 0;

        // Re-initialize dialogue lines if empty
        if (dialogueLines.Count == 0)
        {
            InitializeDialogue();
        }

        EnsureButtonListener();
        ShowCurrentLine();
    }

    /// <summary>
    /// Re-binds the Next button to this instance so clicks are handled (fixes runtime-created UI).
    /// </summary>
    private void EnsureButtonListener()
    {
        if (nextButton == null) return;
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(AdvanceDialogue);
        nextButton.interactable = true;
    }

    private void FindReferences()
    {
        // Try to find the dialogue panel by name (panel is inactive at start, so Find() would miss it)
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Transform panelTransform = FindChildByName(canvas.transform, "DialoguePanel");
            if (panelTransform != null)
            {
                dialoguePanel = panelTransform.gameObject;
                Debug.Log("DialogueManager: Found DialoguePanel");

                // Find child components (panel is inactive, so use FindChildByName)
                if (speakerNameText == null)
                {
                    Transform nameTransform = FindChildByName(panelTransform, "SpeakerNameText");
                    if (nameTransform != null)
                        speakerNameText = nameTransform.GetComponent<TextMeshProUGUI>();
                }

                if (dialogueText == null)
                {
                    Transform textTransform = FindChildByName(panelTransform, "DialogueText");
                    if (textTransform != null)
                        dialogueText = textTransform.GetComponent<TextMeshProUGUI>();
                }

                if (nextButton == null)
                {
                    Transform btnTransform = FindChildByName(panelTransform, "DialogueNextButton");
                    if (btnTransform != null)
                    {
                        nextButton = btnTransform.GetComponent<Button>();
                        if (nextButton != null)
                        {
                            nextButtonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
                            if (nextButtonText != null)
                                nextButtonText.raycastTarget = false;
                            nextButton.onClick.RemoveAllListeners();
                            nextButton.onClick.AddListener(AdvanceDialogue);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Finds a direct child by name even when inactive (Transform.Find skips inactive children).
    /// </summary>
    private static Transform FindChildByName(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
                return child;
        }
        return null;
    }

    private void ShowCurrentLine()
    {
        if (currentLineIndex >= dialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueLines[currentLineIndex];

        if (speakerNameText != null)
        {
            speakerNameText.text = line.speakerName;
            speakerNameText.color = line.isPlayer ? playerNameColor : maskedEntityNameColor;
        }

        currentFullText = line.text;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        typewriterCoroutine = StartCoroutine(TypewriterEffect(line.text));

        UpdateNextButtonText();
    }

    private IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        UpdateNextButtonText();
    }

    private void UpdateNextButtonText()
    {
        if (nextButtonText == null) return;

        if (isTyping)
        {
            nextButtonText.text = "Skip";
        }
        else if (currentLineIndex >= dialogueLines.Count - 1)
        {
            nextButtonText.text = "Start";
        }
        else
        {
            nextButtonText.text = "Next";
        }
    }

    /// <summary>
    /// Called when the Next/Start button is clicked. Public so the Button can invoke it reliably.
    /// </summary>
    public void AdvanceDialogue()
    {
        if (isTyping)
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
            }
            if (dialogueText != null)
                dialogueText.text = currentFullText;
            isTyping = false;
            UpdateNextButtonText();
        }
        else
        {
            currentLineIndex++;
            ShowCurrentLine();
        }
    }

    public void OnNextClicked()
    {
        AdvanceDialogue();
    }

    private void EndDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        OnDialogueCompleted?.Invoke();
    }

    public void SetReferences(GameObject panel, TextMeshProUGUI nameText, TextMeshProUGUI contentText, Button button)
    {
        dialoguePanel = panel;
        speakerNameText = nameText;
        dialogueText = contentText;
        nextButton = button;

        if (nextButton != null)
        {
            nextButtonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
            if (nextButtonText != null)
                nextButtonText.raycastTarget = false;
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(AdvanceDialogue);
            nextButton.interactable = true;
        }
    }

    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }

    /// <summary>
    /// Ensures a DialogueManager and DialoguePanel exist on the given canvas.
    /// Call this when the scene may not have been set up with the dialogue system (e.g. old scene).
    /// </summary>
    public static DialogueManager EnsureOnCanvas(Transform canvasTransform)
    {
        if (canvasTransform == null) return null;

        if (UnityEngine.Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        DialogueManager dm = canvasTransform.GetComponent<DialogueManager>();
        if (dm == null)
            dm = canvasTransform.gameObject.AddComponent<DialogueManager>();

        if (dm.dialoguePanel == null)
        {
            dm.FindReferences();
            if (dm.dialoguePanel == null)
                CreateDialoguePanelRuntime(canvasTransform, dm);
        }

        return dm;
    }

    private static void CreateDialoguePanelRuntime(Transform parent, DialogueManager dm)
    {
        GameObject panel = new GameObject("DialoguePanel");
        panel.transform.SetParent(parent, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.35f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        panelBg.raycastTarget = false;

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.3f, 0.3f, 0.4f, 1f);
        outline.effectDistance = new Vector2(3, 3);

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
        nameText.fontSize = 48;
        nameText.fontStyle = TMPro.FontStyles.Bold;
        nameText.color = new Color(0.4f, 0.7f, 1f);
        nameText.alignment = TMPro.TextAlignmentOptions.Left;

        GameObject dialogueObj = new GameObject("DialogueText");
        dialogueObj.transform.SetParent(panel.transform, false);
        RectTransform dialogueRect = dialogueObj.AddComponent<RectTransform>();
        dialogueRect.anchorMin = new Vector2(0, 0);
        dialogueRect.anchorMax = new Vector2(1, 1);
        dialogueRect.offsetMin = new Vector2(30, 60);
        dialogueRect.offsetMax = new Vector2(-30, -20);
        TextMeshProUGUI dialogueText = dialogueObj.AddComponent<TextMeshProUGUI>();
        dialogueText.text = "";
        dialogueText.fontSize = 36;
        dialogueText.color = Color.white;
        dialogueText.alignment = TMPro.TextAlignmentOptions.TopLeft;
        dialogueText.enableWordWrapping = true;

        GameObject nextBtn = new GameObject("DialogueNextButton");
        nextBtn.transform.SetParent(panel.transform, false);
        RectTransform btnRect = nextBtn.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0);
        btnRect.anchorMax = new Vector2(1, 0);
        btnRect.pivot = new Vector2(1, 0);
        btnRect.anchoredPosition = new Vector2(-20, 15);
        btnRect.sizeDelta = new Vector2(140, 50);
        Image btnImg = nextBtn.AddComponent<Image>();
        btnImg.color = new Color(0.25f, 0.45f, 0.65f);
        Button nextButton = nextBtn.AddComponent<Button>();

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(nextBtn.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Next";
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontSize = 28;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        if (nameText != null) nameText.raycastTarget = false;
        if (dialogueText != null) dialogueText.raycastTarget = false;

        dm.SetReferences(panel, nameText, dialogueText, nextButton);
        panel.SetActive(false);
        Debug.Log("DialogueManager: Created DialoguePanel at runtime (was missing).");
    }
}
