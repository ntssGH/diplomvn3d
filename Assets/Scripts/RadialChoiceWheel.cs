using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class RadialChoiceWheel : MonoBehaviour
{
    [Header("References")]
    public RectTransform wheelRoot;
    public RectTransform segmentsRoot;
    public CanvasGroup canvasGroup;
    public TMP_FontAsset labelFont;

    [Header("Geometry")]
    public float outerRadius = 420f;
    public float innerRadius = 140f;
    public float labelRadius = 280f;
    public float startAngleDeg = -90f;
    [Tooltip("Gap between slices in degrees")]
    [Range(0f, 20f)] public float gapDegrees = 2.5f;

    [Header("Appearance")]
    public Color segmentColor = new Color(1f, 1f, 1f, 0.18f);
    public Color segmentHoverColor = new Color(1f, 1f, 1f, 0.42f);
    public Color labelColor = Color.white;
    public Color labelHoverColor = new Color(1f, 0.92f, 0.3f);

    [Header("Input")]
    public KeyCode confirmKey = KeyCode.Mouse0;
    public KeyCode altConfirmKey = KeyCode.Return;
    public bool allowNumberHotkeys = true;

    [Header("Runtime")]
    public List<string> options = new();
    public UnityEvent<int> onChoice;

    readonly List<RadialChoiceSegmentGraphic> segmentGraphics = new();
    readonly List<TextMeshProUGUI> labels = new();
    int hoveredIndex = -1;
    Canvas cachedCanvas;
    Coroutine selectionRoutine;

    void Awake()
    {
        if (!wheelRoot) wheelRoot = GetComponent<RectTransform>();
        if (!segmentsRoot) segmentsRoot = wheelRoot;
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        cachedCanvas = GetComponentInParent<Canvas>();
        Hide();
    }

    void OnEnable()
    {
        if (canvasGroup) canvasGroup.alpha = 1f;
    }

    public void BuildAndShow(List<string> opts)
    {
        options = opts != null && opts.Count > 0 ? new List<string>(opts) : options;
        if (selectionRoutine != null)
        {
            StopCoroutine(selectionRoutine);
            selectionRoutine = null;
        }
        ClearSegments();
        if (options.Count > 0)
        {
            BuildSegments();
            hoveredIndex = -1;
            gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (selectionRoutine != null)
        {
            StopCoroutine(selectionRoutine);
            selectionRoutine = null;
        }
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        if (wheelRoot)
            wheelRoot.localScale = Vector3.one;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy || options.Count == 0)
            return;

        UpdateHoverIndex();

        if (allowNumberHotkeys)
        {
            for (int i = 0; i < options.Count && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    Choose(i);
                    return;
                }
            }
        }

        if ((Input.GetKeyDown(confirmKey) || Input.GetKeyDown(altConfirmKey)) && hoveredIndex >= 0)
        {
            Choose(hoveredIndex);
        }
    }

    void BuildSegments()
    {
        float sector = 360f / options.Count;
        float halfGap = Mathf.Clamp(gapDegrees, 0f, sector - 0.01f) * 0.5f;

        for (int i = 0; i < options.Count; i++)
        {
            float start = startAngleDeg + i * sector + halfGap;
            float end = startAngleDeg + (i + 1) * sector - halfGap;

            var segGO = new GameObject($"Segment_{i}", typeof(RectTransform), typeof(RadialChoiceSegmentGraphic));
            segGO.transform.SetParent(segmentsRoot, false);
            var segRect = segGO.GetComponent<RectTransform>();
            segRect.sizeDelta = new Vector2(outerRadius * 2f, outerRadius * 2f);
            segRect.anchoredPosition = Vector2.zero;

            var graphic = segGO.GetComponent<RadialChoiceSegmentGraphic>();
            graphic.raycastTarget = false;
            graphic.Configure(start, end, innerRadius, outerRadius, segmentColor);
            segmentGraphics.Add(graphic);

            var labelGO = new GameObject($"Label_{i}", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(segmentsRoot, false);
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(520f, 96f);
            labelRect.anchorMin = labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.localRotation = Quaternion.identity;

            var tmp = labelGO.GetComponent<TextMeshProUGUI>();
            if (labelFont) tmp.font = labelFont;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 22;
            tmp.fontSizeMax = 48;
            tmp.color = labelColor;
            tmp.text = options[i];
            tmp.raycastTarget = false;
            labels.Add(tmp);

            var bender = labelGO.AddComponent<RadialTextBender>();
            bender.Configure(start, end, labelRadius);
        }

        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = false;
        }

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    void ClearSegments()
    {
        if (segmentsRoot)
        {
            for (int i = segmentsRoot.childCount - 1; i >= 0; i--)
                Destroy(segmentsRoot.GetChild(i).gameObject);
        }

        segmentGraphics.Clear();
        labels.Clear();
        hoveredIndex = -1;
    }

    void UpdateHoverIndex()
    {
        int index = GetPointerIndex();
        if (index == hoveredIndex)
            return;

        hoveredIndex = index;
        for (int i = 0; i < segmentGraphics.Count; i++)
        {
            bool active = i == hoveredIndex;
            var g = segmentGraphics[i];
            g.color = active ? segmentHoverColor : segmentColor;
            g.SetVerticesDirty();

            if (i < labels.Count)
            {
                labels[i].color = active ? labelHoverColor : labelColor;
                labels[i].fontSize = active ? 46 : 40;
            }
        }
    }

    int GetPointerIndex()
    {
        Camera eventCam = GetEventCamera();
        if (!wheelRoot || !RectTransformUtility.ScreenPointToLocalPointInRectangle(wheelRoot, Input.mousePosition, eventCam, out Vector2 local))
            return -1;

        float dist = local.magnitude;
        if (dist < innerRadius || dist > outerRadius)
            return -1;

        float angle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;

        if (options.Count == 0)
            return -1;

        float sector = 360f / options.Count;
        float a = (angle - startAngleDeg) % 360f;
        if (a < 0f) a += 360f;

        int idx = Mathf.FloorToInt(a / sector);
        return Mathf.Clamp(idx, 0, options.Count - 1);
    }

    Camera GetEventCamera()
    {
        if (cachedCanvas == null)
            cachedCanvas = GetComponentInParent<Canvas>();
        return cachedCanvas != null ? cachedCanvas.worldCamera : null;
    }

    void Choose(int index)
    {
        if (selectionRoutine != null)
            StopCoroutine(selectionRoutine);

        selectionRoutine = StartCoroutine(PlaySelectionAnimation(index));
        onChoice?.Invoke(index);
    }

    System.Collections.IEnumerator PlaySelectionAnimation(int index)
    {
        float duration = 0.22f;
        float popScale = 1.08f;
        float elapsed = 0f;

        var seg = index >= 0 && index < segmentGraphics.Count ? segmentGraphics[index] : null;
        var label = index >= 0 && index < labels.Count ? labels[index] : null;
        Color segStart = seg ? seg.color : Color.white;
        Color segEnd = seg ? segmentHoverColor : segStart;
        Color labelStart = label ? label.color : Color.white;
        Color labelEnd = label ? labelHoverColor : labelStart;
        float labelBaseSize = label ? label.fontSize : 0f;

        if (canvasGroup)
            canvasGroup.interactable = false;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            float pulse = Mathf.Sin(t * Mathf.PI);

            if (wheelRoot)
                wheelRoot.localScale = Vector3.Lerp(Vector3.one, Vector3.one * popScale, pulse);

            if (canvasGroup)
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, eased);

            if (seg)
            {
                seg.color = Color.Lerp(segStart, segEnd, eased);
                seg.SetVerticesDirty();
            }

            if (label)
            {
                label.color = Color.Lerp(labelStart, labelEnd, eased);
                label.fontSize = Mathf.Lerp(labelBaseSize, 48f, eased);
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (wheelRoot)
            wheelRoot.localScale = Vector3.one;

        Hide();
        selectionRoutine = null;
    }
}
