using UnityEngine;

[ExecuteAlways]
public class WorldSpaceHudScaler : MonoBehaviour
{
    public Camera cam;
    [Range(0.05f, 0.5f)]
    public float screenHeightFraction = 0.18f; // доля высоты экрана
    public RectTransform rootRect;             // RectTransform Canvas
    public float pixelsPerUnit = 100f;         // Canvas->Reference Pixels Per Unit

    void LateUpdate()
    {
        if (!cam) cam = Camera.main;
        if (!rootRect || !cam) return;

        float rectHeightMetersAtScale1 = rootRect.rect.height / pixelsPerUnit;
        float d = Vector3.Distance(transform.position, cam.transform.position);
        float viewHeightAtD = 2f * d * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float desiredWorldHeight = viewHeightAtD * screenHeightFraction;
        float s = desiredWorldHeight / rectHeightMetersAtScale1;
        transform.localScale = Vector3.one * s;
    }
}
