using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class RadialTextBender : MonoBehaviour
{
    [SerializeField] float radius = 320f;
    [SerializeField, Tooltip("Начальный угол в градусах (0° вправо, -90° вверх)")]
    float startAngleDeg = -90f;

    TMP_Text text;

    void Awake() => text = GetComponent<TMP_Text>();

    void OnEnable() => Bend();

    void OnValidate() => Bend();

    public void Bend()
    {
        if (!text) text = GetComponent<TMP_Text>();
        if (!text) return;
        if (Mathf.Approximately(radius, 0f)) return;

        text.ForceMeshUpdate();
        var textInfo = text.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            var verts = textInfo.meshInfo[materialIndex].vertices;

            Vector3 charMid = (verts[vertexIndex] + verts[vertexIndex + 2]) * 0.5f;
            Vector3 offset = charMid;

            for (int j = 0; j < 4; j++)
                verts[vertexIndex + j] -= offset;

            float angRad = startAngleDeg * Mathf.Deg2Rad + (charMid.x / radius);
            float angDeg = angRad * Mathf.Rad2Deg;

            float rotDeg = angDeg - 90f;
            if (Mathf.Cos(angRad) < 0f)
                rotDeg += 180f;

            Quaternion rot = Quaternion.Euler(0f, 0f, rotDeg);
            Vector3 curvedPos = new Vector3(Mathf.Cos(angRad), Mathf.Sin(angRad), 0f) * radius;

            for (int j = 0; j < 4; j++)
                verts[vertexIndex + j] = rot * verts[vertexIndex + j] + curvedPos;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            text.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
