using UnityEngine;

public class SimpleMouseLook : MonoBehaviour
{
    public Transform pitch;      // сюда перетащи камеру (child)
    public float sensX = 140f;
    public float sensY = 140f;
    public float minPitch = -85f, maxPitch = 85f;

    float yaw, pitchAng;

    void Start()
    {
        if (!pitch && Camera.main) pitch = Camera.main.transform;
        Cursor.lockState = CursorLockMode.Locked; // курсор спрятан, камера всегда вертится
        Cursor.visible = false;
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        if (pitch) pitchAng = pitch.localEulerAngles.x;
        if (pitchAng > 180f) pitchAng -= 360f;
    }

    void Update()
    {
        float dt = (Time.timeScale == 0f) ? Time.unscaledDeltaTime : Time.deltaTime;
        float mx = Input.GetAxisRaw("Mouse X");
        float my = Input.GetAxisRaw("Mouse Y");

        yaw      += mx * sensX * dt;
        pitchAng -= my * sensY * dt;
        pitchAng = Mathf.Clamp(pitchAng, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (pitch) pitch.localRotation = Quaternion.Euler(pitchAng, 0f, 0f);
    }
}
