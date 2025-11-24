using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RadialChoiceWheel : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform wheelRoot;      // RectTransform Canvas'а колеса
    public RectTransform segmentsRoot;   // пустышка "Segments"
    public Sprite circleSprite;          // круглый UISprite (без рисунков)
    public TMP_FontAsset font;           // шрифт для подписей
    public UIRingSector hoverRing;       // перетащи объект с UIRingSector

    [Header("Look")]
    public float radius = 450f;          // радиус "пирожка"
    public float labelRadius = 300f;     // радиус подписей
    public Color segNormal = new Color(1, 1, 1, 0.15f);
    public Color segHover = new Color(1, 1, 1, 0.40f);
    public Color labelNormal = Color.white;
    public Color labelHover = Color.yellow;

    [Header("Angles")]
    [Tooltip("0° вправо. -90° вверх.")]
    public float startAngleDeg = -90f;

    [Header("Dividers (optional)")]
    public bool showDividers = false;
    public float dividerThickness = 6f;
    public Color dividerColor = new Color(1, 1, 1, 0.25f);

    [Header("Hover Ring")]
    public float hoverRingOffset = 18f;   // смещение кольца от labelRadius
    public float hoverRingThickness = 28f;
    public float hoverFillPercent = 0.84f; // доля ширины сектора (0..1)

    [Header("Input")]
    public bool useGaze = true;                 // наведение взглядом
    public bool useMouse = true;                // наведение мышкой
    public KeyCode confirmKey = KeyCode.E;      // подтвердить
    public KeyCode altConfirmKey = KeyCode.Space;

    [Header("Runtime")]
    public List<string> options = new();
    public UnityEvent<int> onChoice;

    // internals
    Camera cam;
    RectTransform rect;
    readonly List<Image> segImgs = new();
    readonly List<TextMeshProUGUI> labels = new();
    int hovered = -1;

    float SectorAngle => options.Count > 0 ? 360f / options.Count : 0f;

    void Awake()
    {
        rect = wheelRoot ? wheelRoot : GetComponent<RectTransform>();
        if (!segmentsRoot) segmentsRoot = rect;
    }

    public void BuildAndShow(List<string> opts, Camera uiCam = null)
    {
        options = (opts != null && opts.Count > 0) ? opts : options;
        cam = uiCam ? uiCam : Camera.main;

        ClearSegments();
        if (options.Count > 0) BuildSegments();

        DisableAllRaycasts();
        PrepareHoverRing();

        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);

    void Update()
    {
        if (!gameObject.activeInHierarchy || options.Count == 0) return;

        // наведение мышкой приоритетнее, затем взглядом как запасной вариант
        int idx = -1;
        Vector3 hitPoint = transform.position;

        if (useMouse && cam)
        {
            idx = PointerHoverIndex(cam, Input.mousePosition);
        }
        if (idx < 0 && useGaze && cam)
        {
            idx = GazeHoverIndex(cam, out hitPoint);
            // В Game-вью включи "Gizmos", если хочешь видеть линию:
            Debug.DrawLine(cam.transform.position, hitPoint, Color.cyan, 0f, true);
        }

        SetHover(idx);

        // хоткеи 1..9
        for (int i = 0; i < options.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
            { Choose(i); return; }
        }

        // подтверждение E/Space
        if ((Input.GetKeyDown(confirmKey) || Input.GetKeyDown(altConfirmKey)) && hovered >= 0)
        { Choose(hovered); return; }
    }

    // === Geometry ===
    void BuildSegments()
    {
        float sector = SectorAngle;

        for (int i = 0; i < options.Count; i++)
        {
            CreateSegmentVisual(i, sector);
            CreateLabel(i, sector);
            if (showDividers) CreateDivider(i, sector);
        }
    }

    void ClearSegments()
    {
        for (int i = segmentsRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(segmentsRoot.GetChild(i).gameObject);
        }

        segImgs.Clear();
        labels.Clear();
        hovered = -1;
    }

    // === Hover/Select ===
    void SetHover(int idx)
    {
        if (hovered == idx) return;
        hovered = idx;

        for (int i = 0; i < segImgs.Count; i++)
        {
            bool on = (i == idx);
            segImgs[i].color = on ? segHover : segNormal;

            if (i < labels.Count)
            {
                labels[i].color = on ? labelHover : labelNormal;
                labels[i].fontSize = on ? 44 : 38;
            }
        }

        // тонкая дуга внутри кольца
        if (hoverRing)
        {
            if (idx >= 0 && options.Count > 0)
            {
                float midDeg = startAngleDeg + (idx + 0.5f) * SectorAngle;
                float span = SectorAngle * Mathf.Clamp01(hoverFillPercent);
                float a0 = midDeg - span * 0.5f;
                float a1 = midDeg + span * 0.5f;

                hoverRing.enabled = true;
                hoverRing.SetArc(a0, a1, labelRadius + hoverRingOffset, hoverRingThickness);
            }
            else hoverRing.enabled = false;
        }
    }

    void Choose(int index)
    {
        onChoice?.Invoke(index);
        Hide();
    }

    // === Helpers ===
    void DisableAllRaycasts()
    {
        foreach (var g in GetComponentsInChildren<Graphic>(true))
            g.raycastTarget = false;
    }

    void PrepareHoverRing()
    {
        if (!hoverRing) return;

        hoverRing.enabled = false;
        hoverRing.color = new Color(1, 1, 1, 0.6f);
        hoverRing.outerRadius = labelRadius + hoverRingOffset;
        hoverRing.thickness = hoverRingThickness;
    }

    void CreateSegmentVisual(int index, float sector)
    {
        var segGO = new GameObject($"Seg_{index}", typeof(RectTransform), typeof(Image));
        segGO.transform.SetParent(segmentsRoot, false);

        var sr = segGO.GetComponent<RectTransform>();
        sr.sizeDelta = new Vector2(radius * 2f, radius * 2f);
        sr.anchoredPosition = Vector2.zero;
        sr.localRotation = Quaternion.Euler(0, 0, -(startAngleDeg + index * sector));

        var img = segGO.GetComponent<Image>();
        img.sprite = circleSprite;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillOrigin = 2;      // сверху
        img.fillClockwise = false;
        img.fillAmount = 1f / options.Count;
        img.color = segNormal;
        img.raycastTarget = false;
        segImgs.Add(img);
    }

    void CreateLabel(int index, float sector)
    {
        var labelGO = new GameObject($"Label_{index}", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(segmentsRoot, false);

        var lr = labelGO.GetComponent<RectTransform>();
        lr.sizeDelta = new Vector2(520, 90);

        float midDeg = startAngleDeg + (index + 0.5f) * sector;
        float midRad = midDeg * Mathf.Deg2Rad;
        lr.anchoredPosition = new Vector2(Mathf.Cos(midRad), Mathf.Sin(midRad)) * labelRadius;
        lr.localRotation = Quaternion.identity;

        var tmp = labelGO.GetComponent<TextMeshProUGUI>();
        if (font) tmp.font = font;
        tmp.text = $"{index + 1}) {options[index]}";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 22;
        tmp.fontSizeMax = 48;
        tmp.color = labelNormal;
        tmp.raycastTarget = false;
        labels.Add(tmp);
    }

    void CreateDivider(int index, float sector)
    {
        var divGO = new GameObject($"Divider_{index}", typeof(RectTransform), typeof(Image));
        divGO.transform.SetParent(segmentsRoot, false);

        var dr = divGO.GetComponent<RectTransform>();
        dr.sizeDelta = new Vector2(dividerThickness, radius * 2f);
        dr.anchoredPosition = Vector2.zero;

        float edgeDeg = startAngleDeg + index * sector;
        dr.localRotation = Quaternion.Euler(0, 0, -edgeDeg);

        var dimg = divGO.GetComponent<Image>();
        dimg.color = dividerColor;
        dimg.raycastTarget = false;
    }

    // === Ray/Angle math ===
    int PointerHoverIndex(Camera c, Vector2 screenPos)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, c, out var local))
            return HoverIndexFromLocal(local);
        return -1;
    }

    int GazeHoverIndex(Camera c, out Vector3 worldHit)
    {
        worldHit = transform.position;

        var plane = new Plane(transform.forward, transform.position);
        Ray ray = new Ray(c.transform.position, c.transform.forward);
        if (!plane.Raycast(ray, out float dist)) return -1;

        worldHit = ray.GetPoint(dist);

        Vector3 local = transform.InverseTransformPoint(worldHit);
        Vector2 p = new Vector2(local.x, local.y);
        if (p.sqrMagnitude < 1e-6f) return -1;

        float angle = Mathf.Atan2(p.y, p.x) * Mathf.Rad2Deg; // 0° по +X
        if (angle < 0f) angle += 360f;

        return IndexFromAngle(angle);
    }

    int HoverIndexFromLocal(Vector2 local)
    {
        if (local.sqrMagnitude < 1e-6f || options.Count == 0) return -1;

        float angle = Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;

        return IndexFromAngle(angle);
    }

    int IndexFromAngle(float absoluteAngle)
    {
        if (options.Count == 0) return -1;

        float offsetAngle = (absoluteAngle - startAngleDeg) % 360f;
        if (offsetAngle < 0f) offsetAngle += 360f;

        int idx = Mathf.FloorToInt(offsetAngle / SectorAngle);
        return Mathf.Clamp(idx, 0, options.Count - 1);
    }
}
