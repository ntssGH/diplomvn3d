using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple radial wedge graphic for the choice wheel. Draws a solid sector between inner and outer radius.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class RadialChoiceSegmentGraphic : MaskableGraphic
{
    [Range(8, 256)] public int segments = 96;
    [Min(0f)] public float innerRadius = 140f;
    [Min(0f)] public float outerRadius = 420f;
    public float startDeg = -90f;
    public float endDeg = -45f;

    public void Configure(float start, float end, float inner, float outer, Color32 col)
    {
        startDeg = start;
        endDeg = end;
        innerRadius = Mathf.Max(0f, inner);
        outerRadius = Mathf.Max(innerRadius, outer);
        color = col;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float a0 = startDeg * Mathf.Deg2Rad;
        float a1 = endDeg * Mathf.Deg2Rad;
        if (a1 < a0)
        {
            float t = a0;
            a0 = a1;
            a1 = t;
        }

        float span = Mathf.Max(0.001f, a1 - a0);
        int segCount = Mathf.Max(2, segments);
        int vertexPairs = segCount + 1;

        for (int i = 0; i < vertexPairs; i++)
        {
            float t = i / (float)(vertexPairs - 1);
            float ang = a0 + span * t;
            float ca = Mathf.Cos(ang);
            float sa = Mathf.Sin(ang);

            Vector3 outerPos = new Vector3(ca * outerRadius, sa * outerRadius, 0f);
            Vector3 innerPos = new Vector3(ca * innerRadius, sa * innerRadius, 0f);

            AddVert(vh, outerPos, color);
            AddVert(vh, innerPos, color);
        }

        for (int i = 0; i < segCount; i++)
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
