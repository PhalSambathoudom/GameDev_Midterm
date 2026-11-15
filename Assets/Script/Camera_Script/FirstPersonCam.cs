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
    private float pitch;

    void Awake()
    {
        playerMap = inputActions.FindActionMap("Player", true);
        lookAction = playerMap.FindAction("Look", true);
    }

    void OnEnable()
    {
        playerMap.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable() => playerMap.Disable();

    void LateUpdate()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            // click to re-lock after pressing Escape in the editor
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            return;
        }

        Vector2 look = lookAction.ReadValue<Vector2>();
        bool mouse = lookAction.activeControl != null && lookAction.activeControl.device is Mouse;
        Vector2 delta = mouse ? look * mouseSensitivity : look * gamepadSensitivity * Time.deltaTime;

        playerBody.Rotate(Vector3.up * delta.x);
        pitch = Mathf.Clamp(pitch - delta.y, -maxLookAngle, maxLookAngle);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        if (Time.timeScale == 0f) return;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Time.timeScale == 0f) return;
    }
}