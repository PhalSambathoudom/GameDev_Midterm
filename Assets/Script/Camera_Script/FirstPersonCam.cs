using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCam : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Transform playerBody;          
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float gamepadSensitivity = 120f; 
    [SerializeField] private float maxLookAngle = 85f;

    private InputActionMap playerMap;
    private InputAction lookAction;
    private Rigidbody playerRigidbody;
    private float pitch;
    private float pendingYaw;

    void Awake()
    {
        if (inputActions == null)
        {
            Debug.LogError("Assign the FirstPersonCam Input Actions asset.", this);
            enabled = false;
            return;
        }

        if (playerBody == null)
        {
            Debug.LogError("Assign the Player Body transform to FirstPersonCam.", this);
            enabled = false;
            return;
        }

        playerRigidbody = playerBody.GetComponent<Rigidbody>();
        if (playerRigidbody == null)
        {
            Debug.LogError("The Player Body must have a Rigidbody.", this);
            enabled = false;
            return;
        }

        playerMap = inputActions.FindActionMap("Player", false);
        lookAction = playerMap != null ? playerMap.FindAction("Look", false) : null;
        if (lookAction == null)
        {
            Debug.LogError("The Input Actions asset must contain a Player/Look action.", this);
            enabled = false;
        }
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable() => pendingYaw = 0f;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Time.timeScale <= 0f)
        {
            pendingYaw = 0f;
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            return;
        }

        if (lookAction == null) return;
        Vector2 look = lookAction.ReadValue<Vector2>();
        bool mouse = lookAction.activeControl != null && lookAction.activeControl.device is Mouse;
        Vector2 delta = mouse ? look * mouseSensitivity : look * gamepadSensitivity * Time.deltaTime;

        pendingYaw += delta.x;
        pitch = Mathf.Clamp(pitch - delta.y, -maxLookAngle, maxLookAngle);
    }

    void FixedUpdate()
    {
        if (Time.timeScale <= 0f || playerRigidbody == null)
        {
            pendingYaw = 0f;
            return;
        }

        if (pendingYaw == 0f) return;
        playerRigidbody.MoveRotation(
            playerRigidbody.rotation * Quaternion.Euler(0f, pendingYaw, 0f));
        pendingYaw = 0f;
    }

    void LateUpdate() => transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
}