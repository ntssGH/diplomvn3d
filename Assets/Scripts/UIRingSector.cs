using UnityEngine;
using UnityEngine.UI;

/// Тонкая дуга в Canvas (Screen/World Space). Идеальна как hover-подсветка.
[RequireComponent(typeof(CanvasRenderer))]
public class UIRingSector : MaskableGraphic
{
    [Range(8, 256)] public int segments = 96;
    [Tooltip("Внешний радиус дуги (в единицах RectTransform)")]
    public float outerRadius = 320f;
    [Tooltip("Толщина кольца (в тех же единицах)")]
    public float thickness = 24f;

    [Tooltip("Начало/конец дуги в градусах. 0° вправо, -90° вверх.")]
    public float startDeg = -20f;
    public float endDeg = 20f;

    public void SetArc(float a0, float a1, float outerR, float thick)
    {
        startDeg = a0;
        endDeg = a1;
        outerRadius = outerR;
        thickness = Mathf.Max(0.5f, thick);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float innerR = Mathf.Max(0f, outerRadius - thickness);

        float a0 = startDeg * Mathf.Deg2Rad;
        float a1 = endDeg * Mathf.Deg2Rad;
        if (a1 < a0) { var t = a0; a0 = a1; a1 = t; }
        float span = Mathf.Max(0.001f, a1 - a0);

        int seg = Mathf.Max(2, segments);
        int vcount = (seg + 1) * 2;

        for (int i = 0; i <= seg; i++)
        {
            float t = i / (float)seg;
            float a = a0 + span * t;
            float ca = Mathf.Cos(a);
            float sa = Mathf.Sin(a);

            Vector3 pOuter = new Vector3(ca * outerRadius, sa * outerRadius, 0f);
            Vector3 pInner = new Vector3(ca * innerR, sa * innerR, 0f);

            AddVert(vh, pOuter, color);
            AddVert(vh, pInner, color);
        }

        for (int i = 0; i < seg; i++)
        {
            int i0 = i * 2;
            int i1 = i0 + 1;
            int i2 = i0 + 2;
            int i3 = i0 + 3;
            vh.AddTriangle(i0, i2, i1);
            vh.AddTriangle(i2, i3, i1);
        }
    }

    static void AddVert(VertexHelper vh, Vector3 pos, Color32 col)
    {
        var v = UIVertex.simpleVert;
        v.position = pos;
        v.color = col;
        vh.AddVert(v);
    }
}
