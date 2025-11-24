using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Builds a simple start screen and pause menu at runtime.
/// </summary>
[DefaultExecutionOrder(-500)]
public class GameFlowUI : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindObjectOfType<GameFlowUI>() != null)
            return;

        var go = new GameObject(nameof(GameFlowUI));
        go.AddComponent<GameFlowUI>();
    }

    [Header("Appearance")]
    public Color overlayColor = new Color(0f, 0f, 0f, 0.6f);
    public Color panelColor = new Color(0.09f, 0.09f, 0.12f, 0.8f);
    public Color buttonColor = new Color(0.18f, 0.42f, 0.85f, 0.95f);
    public TMP_FontAsset fontAsset;

    [Header("Controls")]
    public KeyCode pauseKey = KeyCode.Escape;

    Canvas rootCanvas;
    GameObject startPanel;
    GameObject pausePanel;
    Transform startContent;
    Transform pauseContent;
    Button startButton;
    Button resumeButton;
    Button restartButton;
    TimeFreeze timeFreeze;
    bool gameStarted;
    float previousTimeScale = 1f;
    bool previousAudioPause;

    void Awake()
    {
        SetupCanvas();
        EnsureEventSystem();
        BuildStartPanel();
        BuildPausePanel();

        timeFreeze = gameObject.AddComponent<TimeFreeze>();
        ShowStartScreen();
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!gameStarted)
            return;

        if (Input.GetKeyDown(pauseKey))
        {
            if (pausePanel.activeSelf)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void SetupCanvas()
    {
        rootCanvas = new GameObject("GameFlowCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<Canvas>();
        rootCanvas.transform.SetParent(transform, false);
        rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        rootCanvas.sortingOrder = 9999;

        var scaler = rootCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
    }

    void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
            return;

        var evt = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        DontDestroyOnLoad(evt);
    }

    void BuildStartPanel()
    {
        startContent = CreateOverlay("StartPanel", out startPanel);

        var title = CreateLabel("ChoiceWheel", 72, FontStyles.Bold, startContent);
        title.rectTransform.anchoredPosition = new Vector2(0f, 160f);

        startButton = CreateButton("Начать", StartGame, startContent);
        startButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f);
    }

    void BuildPausePanel()
    {
        pauseContent = CreateOverlay("PausePanel", out pausePanel);

        var title = CreateLabel("Пауза", 60, FontStyles.Bold, pauseContent);
        title.rectTransform.anchoredPosition = new Vector2(0f, 120f);

        resumeButton = CreateButton("Продолжить", ResumeGame, pauseContent);
        resumeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 20f);

        restartButton = CreateButton("С начала", ShowStartScreen, pauseContent);
        restartButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -100f);

        pausePanel.SetActive(false);
    }

    Transform CreateOverlay(string name, out GameObject overlay)
    {
        overlay = new GameObject(name, typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(rootCanvas.transform, false);
        var overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = overlayRect.offsetMax = Vector2.zero;

        var overlayImage = overlay.GetComponent<Image>();
        overlayImage.color = overlayColor;

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(overlay.transform, false);
        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(540f, 420f);
        panelRect.anchoredPosition = Vector2.zero;

        var panelImage = panel.GetComponent<Image>();
        panelImage.color = panelColor;

        return panel.transform;
    }

    TMP_Text CreateLabel(string text, float size, FontStyles style, Transform parent)
    {
        var labelGO = new GameObject(text + "Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(parent, false);

        var rect = labelGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(800f, 160f);

        var tmp = labelGO.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        if (fontAsset)
            tmp.font = fontAsset;
        tmp.color = Color.white;

        return tmp;
    }

    Button CreateButton(string label, UnityAction onClick, Transform parent)
    {
        var buttonGO = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonGO.transform.SetParent(parent, false);

        var rect = buttonGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(320f, 80f);

        var image = buttonGO.GetComponent<Image>();
        image.color = buttonColor;

        var tmp = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        tmp.transform.SetParent(buttonGO.transform, false);
        tmp.rectTransform.sizeDelta = new Vector2(300f, 60f);
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 36f;
        tmp.text = label;
        if (fontAsset)
            tmp.font = fontAsset;

        var btn = buttonGO.GetComponent<Button>();
        btn.onClick.AddListener(onClick);

        return btn;
    }

    void StartGame()
    {
        gameStarted = true;
        startPanel.SetActive(false);
        pausePanel.SetActive(false);
        RestoreTime();
    }

    void PauseGame()
    {
        pausePanel.SetActive(true);
        StoreAndFreezeTime();
    }

    void ResumeGame()
    {
        pausePanel.SetActive(false);
        RestoreTime();
    }

    void ShowStartScreen()
    {
        gameStarted = false;
        startPanel.SetActive(true);
        pausePanel.SetActive(false);
        StoreAndFreezeTime();
    }

    void StoreAndFreezeTime()
    {
        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        timeFreeze.Freeze();
    }

    void RestoreTime()
    {
        AudioListener.pause = previousAudioPause;
        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        timeFreeze.Unfreeze();
    }
}
