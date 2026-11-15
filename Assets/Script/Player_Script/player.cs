using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Player : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions; 

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 7.5f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float exhaustionThreshold = 20f;
    [SerializeField] private float staminaDrainRate = 10f;
    [SerializeField] private float staminaRegenRate = 5f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float groundCheckExtra = 0.1f;

    private Rigidbody rb;
    private CapsuleCollider col;
    private InputActionMap playerMap;
    private InputAction moveAction, sprintAction, jumpAction;

    private Vector2 moveInput;
    private bool jumpQueued;
    private bool isSprinting;
    private bool isExhausted;
    private float stamina;

    public float Stamina => stamina;
    public float MaxStamina => maxStamina;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        rb.freezeRotation = true;
        stamina = maxStamina;

        playerMap = inputActions.FindActionMap("Player", true);
        moveAction = playerMap.FindAction("Move", true);
        sprintAction = playerMap.FindAction("Sprint", true);
        jumpAction = playerMap.FindAction("Jump", true);
    }

    void OnEnable() => playerMap.Enable();
    void OnDisable() { moveInput = Vector2.zero; playerMap.Disable(); }

    void Update()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        if (jumpAction.WasPressedThisFrame()) jumpQueued = true;
        HandleStamina();
    }

    void FixedUpdate() => MovePlayer();

    private void HandleStamina()
    {
        if (isExhausted && stamina >= exhaustionThreshold) isExhausted = false;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        isSprinting = sprintAction.IsPressed() && isMoving && stamina > 0f && !isExhausted;

        if (isSprinting)
        {
            stamina -= staminaDrainRate * Time.deltaTime;
            if (stamina <= 0f) { stamina = 0f; isExhausted = true; isSprinting = false; }
        }
        else
        {
            stamina = Mathf.Min(stamina + staminaRegenRate * Time.deltaTime, maxStamina);
        }
    }

    private void MovePlayer()
    {
        Vector3 dir = transform.right * moveInput.x + transform.forward * moveInput.y;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        Vector3 v = dir * (isSprinting ? sprintSpeed : moveSpeed);
        v.y = rb.linearVelocity.y;
        rb.linearVelocity = v;

        if (jumpQueued)
        {
            jumpQueued = false;
            if (IsGrounded()) rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        float dist = col.bounds.extents.y + groundCheckExtra;
        return Physics.Raycast(col.bounds.center, Vector3.down, dist, groundMask, QueryTriggerInteraction.Ignore);
    }

    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        rb.linearVelocity = Vector3.zero;
        rb.position = position;
        rb.rotation = rotation;
        transform.SetPositionAndRotation(position, rotation);
    }
}
