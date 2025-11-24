using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private Color menuBackgroundColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);

    private Canvas canvas;
    private GameObject mainMenuPanel;
    private GameObject pauseMenuPanel;
    private TimeFreeze timeFreeze;
    private Font defaultFont;
    private bool gameStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateMenuManager()
    {
        if (FindObjectOfType<MenuManager>() != null)
            return;

        var menuManagerObject = new GameObject("MenuManager");
        menuManagerObject.AddComponent<MenuManager>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        timeFreeze = gameObject.AddComponent<TimeFreeze>();

        BuildCanvas();
        BuildMainMenu();
        BuildPauseMenu();
        EnsureEventSystem();
        ShowMainMenu();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureEventSystem();
    }

    private void Update()
    {
        if (!gameStarted)
            return;

        if (Input.GetKeyDown(pauseKey))
            TogglePauseMenu();
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
            return;

        var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
        eventSystemObject.AddComponent<StandaloneInputModule>();
        DontDestroyOnLoad(eventSystemObject);
    }

    private void BuildCanvas()
    {
        var canvasObject = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        DontDestroyOnLoad(canvasObject);

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
    }

    private GameObject CreatePanel(string name)
    {
        var panelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(canvas.transform, false);

        var rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var background = panelObject.GetComponent<Image>();
        background.color = menuBackgroundColor;

        return panelObject;
    }

    private GameObject CreateVerticalGroup(Transform parent)
    {
        var layoutObject = new GameObject("Layout", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        layoutObject.transform.SetParent(parent, false);

        var rect = layoutObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(400f, 0f);

        var layoutGroup = layoutObject.GetComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleCenter;
        layoutGroup.spacing = 16f;

        var fitter = layoutObject.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return layoutObject;
    }

    private Text CreateText(string label, Transform parent, int fontSize = 48, FontStyle fontStyle = FontStyle.Bold)
    {
        var textObject = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        var text = textObject.GetComponent<Text>();
        text.font = defaultFont;
        text.text = label;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;

        var rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400f, fontSize + 12f);

        return text;
    }

    private Button CreateButton(string label, Transform parent, UnityEngine.Events.UnityAction onClick)
    {
        var buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        var image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.85f);

        var button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(onClick);

        var text = CreateText(label, buttonObject.transform, 28, FontStyle.Normal);
        text.color = Color.black;

        var rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(340f, 60f);

        return button;
    }

    private void BuildMainMenu()
    {
        mainMenuPanel = CreatePanel("MainMenuPanel");
        var layout = CreateVerticalGroup(mainMenuPanel.transform);

        CreateText("Главное меню", layout.transform);
        CreateButton("Начать", layout.transform, StartGame);
        CreateButton("Выход", layout.transform, QuitGame);
    }

    private void BuildPauseMenu()
    {
        pauseMenuPanel = CreatePanel("PauseMenuPanel");
        var layout = CreateVerticalGroup(pauseMenuPanel.transform);

        CreateText("Пауза", layout.transform);
        CreateButton("Продолжить", layout.transform, ResumeGame);
        CreateButton("Выход", layout.transform, QuitGame);
    }

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        pauseMenuPanel.SetActive(false);
        timeFreeze.Freeze();
        gameStarted = false;
    }

    private void StartGame()
    {
        mainMenuPanel.SetActive(false);
        pauseMenuPanel.SetActive(false);
        gameStarted = true;
        timeFreeze.Unfreeze();
    }

    private void TogglePauseMenu()
    {
        if (pauseMenuPanel.activeSelf)
        {
            ResumeGame();
        }
        else
        {
            ShowPauseMenu();
        }
    }

    private void ShowPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
        timeFreeze.Freeze();
    }

    private void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        timeFreeze.Unfreeze();
    }

    private void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
