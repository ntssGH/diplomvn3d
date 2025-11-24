using UnityEngine;

public class DialogueBillboardFollow : MonoBehaviour
{
    public Transform cam;
    public Vector3 localOffset = new Vector3(0f, -0.3f, 1.8f); // где держать относительно камеры
    [Tooltip("Время догоняния (сек). Меньше — быстрее.")]
    public float posLag = 0.12f;
    public float rotLag = 0.12f;
    public bool unparentOnStart = true;

    Vector3 vel;

    void Start()
    {
        if (unparentOnStart && transform.parent) transform.SetParent(null, true);
        if (!cam && Camera.main) cam = Camera.main.transform;
        // стартовая позиция сразу в целевую, чтобы не дёрнуло
        if (cam) transform.position = cam.TransformPoint(localOffset);
    }

    void LateUpdate()
    {
        if (!cam) { var m = Camera.main; if (!m) return; cam = m.transform; }

        var targetWorld = cam.TransformPoint(localOffset);

        // Плавное догоняние позиции (в МИРЕ) — даёт инерцию и влево/вправо
        transform.position = Vector3.SmoothDamp(
            transform.position, targetWorld, ref vel,
            Mathf.Max(0.0001f, posLag), Mathf.Infinity, Time.unscaledDeltaTime);

        // Плавный billboard к камере
        var toCam = cam.position - transform.position;
        var targetRot = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
        float k = Mathf.Clamp01(Time.unscaledDeltaTime / Mathf.Max(0.0001f, rotLag));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, k);
    }
}
