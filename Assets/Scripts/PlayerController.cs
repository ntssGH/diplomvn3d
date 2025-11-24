using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 6.0f;
    public float acceleration = 12f;

    [Header("Jump / Gravity")]
    public bool allowJump = true;
    public float jumpHeight = 1.1f;        
    public float gravity = -19.62f;        

    [Header("Crouch")]
    public bool allowCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = 1.2f;
    [Range(0.05f, 0.5f)] public float crouchLerp = 0.15f;

    [Header("Head/Camera (auto)")]
    public Transform head;                 
    public float headStandHeight = 0f;     
    public float headCrouchHeight = 0f;    
    [Range(0.01f, 0.5f)] public float headLerp = 0.12f;

    [Header("Move relative to camera")]
    public Transform cameraTransform;

    [Header("Grounding")]
    public LayerMask groundMask = ~0;
    public float groundProbeExtra = 0.05f;

    CharacterController cc;
    float currentSpeed;
    Vector3 velocity;
    float originalHeight;
    Vector3 originalCenter;
    bool isCrouched;
    float defaultStepOffset;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        originalHeight = cc.height;
        originalCenter = cc.center;
        defaultStepOffset = cc.stepOffset;

        if (!cameraTransform)
        {
            if (Camera.main) cameraTransform = Camera.main.transform;
            else { var cam = GetComponentInChildren<Camera>(); if (cam) cameraTransform = cam.transform; }
        }
        if (!head) head = cameraTransform;

        if (head)
        {
            if (headStandHeight <= 0f) headStandHeight = head.localPosition.y;
            if (headCrouchHeight <= 0f) headCrouchHeight = headStandHeight * 0.65f;
        }
    }

    void Update()
    {
        float dt = (Time.timeScale == 0f) ? Time.unscaledDeltaTime : Time.deltaTime;

        float ix = Input.GetAxisRaw("Horizontal");
        float iy = Input.GetAxisRaw("Vertical");
        Vector2 moveInput = new Vector2(ix, iy);
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
        bool sprint = Input.GetKey(KeyCode.LeftShift);
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);

        Vector3 fwd = transform.forward, right = transform.right;
        if (cameraTransform)
        { fwd = cameraTransform.forward; fwd.y = 0f; fwd.Normalize();
          right = cameraTransform.right; right.y = 0f; right.Normalize(); }
        Vector3 moveDir = fwd * moveInput.y + right * moveInput.x;

        float targetSpeed = (sprint && !isCrouched) ? sprintSpeed : walkSpeed;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * dt);
        cc.Move(moveDir * currentSpeed * dt);

        bool grounded = IsGroundedPrecise();
        cc.stepOffset = grounded ? defaultStepOffset : 0f;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        if (allowJump && grounded && jumpPressed)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        if (Time.timeScale == 0f) velocity.y = 0f;
        else                      velocity.y += gravity * dt;

        cc.Move(velocity * dt);

        if (allowCrouch && Input.GetKeyDown(crouchKey)) isCrouched = !isCrouched;
        float targetH = isCrouched ? crouchHeight : originalHeight;
        cc.height = Mathf.Lerp(cc.height, targetH, 1f - Mathf.Exp(-dt / crouchLerp));
        cc.center = new Vector3(originalCenter.x, cc.height * 0.5f, originalCenter.z);

        if (head)
        {
            float targetY = isCrouched ? headCrouchHeight : headStandHeight;
            float newY = Mathf.Lerp(head.localPosition.y, targetY, 1f - Mathf.Exp(-dt / headLerp));
            head.localPosition = new Vector3(head.localPosition.x, newY, head.localPosition.z);
        }
    }

    bool IsGroundedPrecise()
    {
        Vector3 foot = transform.position + cc.center + Vector3.down * (cc.height * 0.5f - cc.radius + groundProbeExtra);
        float r = cc.radius * 0.95f;
        return Physics.CheckSphere(foot, r, groundMask, QueryTriggerInteraction.Ignore);
    }

    void OnDrawGizmosSelected()
    {
        if (!cc) cc = GetComponent<CharacterController>();
        if (!cc) return;
        Vector3 foot = transform.position + cc.center + Vector3.down * (cc.height * 0.5f - cc.radius + groundProbeExtra);
        float r = cc.radius * 0.95f;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(foot, r);
    }
}
