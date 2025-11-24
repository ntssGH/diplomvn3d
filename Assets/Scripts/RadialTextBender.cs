using TMPro;
using UnityEngine;

/// <summary>
/// Bends a TextMeshProUGUI label along a circular arc to match the radial wheel.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class RadialTextBender : MonoBehaviour
{
    [Min(0f)] public float radius = 280f;
    public float startAngleDeg;
    public float endAngleDeg;
    public Vector2 center = Vector2.zero;

    TMP_Text tmpText;

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (tmpText != null)
            tmpText.havePropertiesChanged = true;
    }

    void LateUpdate()
    {
        if (tmpText == null)
            return;

        if (tmpText.havePropertiesChanged)
        {
            tmpText.havePropertiesChanged = false;
            Bend();
        }
    }

    public void Configure(float start, float end, float r)
    {
        startAngleDeg = start;
        endAngleDeg = end;
        radius = Mathf.Max(0f, r);

        if (tmpText != null)
            tmpText.havePropertiesChanged = true;
    }

    void Bend()
    {
        tmpText.ForceMeshUpdate();
        var textInfo = tmpText.textInfo;
        int charCount = textInfo.characterCount;
        if (charCount == 0)
            return;

        float startRad = startAngleDeg * Mathf.Deg2Rad;
        float endRad = endAngleDeg * Mathf.Deg2Rad;

        float baselineStart = textInfo.characterInfo[0].origin;
        float baselineEnd = textInfo.characterInfo[charCount - 1].xAdvance;
        float baselineWidth = Mathf.Max(0.001f, baselineEnd - baselineStart);

        for (int i = 0; i < charCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible)
                continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vertIndex = charInfo.vertexIndex;
            Vector3[] verts = textInfo.meshInfo[matIndex].vertices;

            Vector3 bl = verts[vertIndex + 0];
            Vector3 tl = verts[vertIndex + 1];
            Vector3 tr = verts[vertIndex + 2];
            Vector3 br = verts[vertIndex + 3];

            Vector3 mid = (bl + tr) * 0.5f;
            float t = (mid.x - baselineStart) / baselineWidth;
            float ang = Mathf.Lerp(startRad, endRad, t);
            // Rotate along the tangent so letters remain upright at the top of the wheel.
            Quaternion rot = Quaternion.Euler(0f, 0f, ang * Mathf.Rad2Deg - 90f);
            Vector3 radialPos = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f) * radius + new Vector3(center.x, center.y, 0f);

            verts[vertIndex + 0] = radialPos + rot * (bl - mid);
            verts[vertIndex + 1] = radialPos + rot * (tl - mid);
            verts[vertIndex + 2] = radialPos + rot * (tr - mid);
            verts[vertIndex + 3] = radialPos + rot * (br - mid);
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            tmpText.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
