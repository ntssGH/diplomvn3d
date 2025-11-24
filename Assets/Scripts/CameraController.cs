using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody; // крутит по Y (обычно корень Player)

    [Header("Look")]
    public float sensitivityX = 120f;
    public float sensitivityY = 120f;
    public float minPitch = -80f;
    public float maxPitch = 80f;
    public bool invertY = false;

    [Header("Smoothing")]
    public bool smoothing = true;
    [Tooltip("Секунд до ~63% цели. 0 = без сглаживания")]
    public float smoothTime = 0.05f;

    [Header("Cursor")]
    public bool lockCursorOnStart = true;
    public KeyCode toggleLockKey = KeyCode.Escape;

    [Header("Zoom (hold RMB)")]
    public bool enableHoldZoom = true;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 45f;
    public float normalFOV = 60f;
    public float zoomLerp = 0.1f;

    float yaw;   // по Y (вправо/влево)
    float pitch; // по X (вверх/вниз)

    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (lockCursorOnStart) LockCursor(true);

        // Инициализируем углы из текущих поворотов
        Vector3 eCam = transform.localEulerAngles;
        pitch = NormalizePitch(eCam.x);
        yaw = (playerBody ? playerBody.eulerAngles.y : transform.eulerAngles.y);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (cam != null) cam.fieldOfView = normalFOV;
    }

    void Update()
    {
        HandleCursorToggle();

        Vector2 look = ReadLookInput();
        float dt = Time.deltaTime;

        yaw += look.x * sensitivityX * dt;
        float ySign = invertY ? 1f : -1f; // стандартно: вверх мышь -> pitch уменьшается
        pitch += ySign * look.y * sensitivityY * dt;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        float alpha = (smoothing && smoothTime > 0f) ? 1f - Mathf.Exp(-dt / smoothTime) : 1f;

        // Применяем yaw: либо к телу игрока, либо к самой камере (если playerBody не задан)
        Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);
        if (playerBody != null)
            playerBody.rotation = Quaternion.Slerp(playerBody.rotation, yawRot, alpha);
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, yawRot, alpha);

        // Применяем pitch на локальный поворот камеры
        Quaternion pitchRot = Quaternion.Euler(pitch, 0f, 0f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, pitchRot, alpha);

        HandleZoom();
    }

    void HandleZoom()
    {
        if (cam == null || !enableHoldZoom) return;

#if ENABLE_INPUT_SYSTEM
        bool held = (UnityEngine.InputSystem.Mouse.current != null &&
                     UnityEngine.InputSystem.Mouse.current.rightButton.isPressed);
#else
        bool held = Input.GetKey(zoomKey);
#endif
        float target = held ? zoomFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, target, 1f - Mathf.Exp(-Time.deltaTime / zoomLerp));
    }

    Vector2 ReadLookInput()
    {
#if ENABLE_INPUT_SYSTEM
        var mouse = UnityEngine.InputSystem.Mouse.current;
        Vector2 v = Vector2.zero;
        if (mouse != null) v += mouse.delta.ReadValue();
        var gp = UnityEngine.InputSystem.Gamepad.current;
        if (gp != null) v += gp.rightStick.ReadValue() * 50f; // подгон под мышь
        return v;
#else
        float x = Input.GetAxisRaw("Mouse X");
        float y = Input.GetAxisRaw("Mouse Y");
        // Если заведёшь оси для стика:
        x += Input.GetAxisRaw("RightStick X");
        y += Input.GetAxisRaw("RightStick Y");
        return new Vector2(x, y);
#endif
    }

    void HandleCursorToggle()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
            LockCursor(Cursor.lockState != CursorLockMode.Locked);
#else
        if (Input.GetKeyDown(toggleLockKey))
            LockCursor(Cursor.lockState != CursorLockMode.Locked);
#endif
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    static float NormalizePitch(float xAngle)
    {
        // 0..360 -> -180..180
        return (xAngle > 180f) ? xAngle - 360f : xAngle;
    }

    public void SetSensitivity(float sx, float sy)
    {
        sensitivityX = sx;
        sensitivityY = sy;
    }
}
