using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private float moveSpeed = 5f;
    private float sprintSpeed = 7.5f;
    private bool isSprinting = false;
    private float maxStamina = 100f;
    private float stamina = 100f;
    private bool isExhausted = false;
    private float exhaustionThreshold = 20f;
    private float jumpForce = 5f;
    private int maxHp = 15;
    private int lives = 5;
    private int inventorySize = 1;
    private bool hasFlag = false;
    private bool autoHealing = false;
    private float autoHealRate = 5f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Handle sprinting and stamina
        if (isExhausted)
        {
            if (stamina >= exhaustionThreshold)
            {
                isExhausted = false;
            }
        }
        isSprinting = Input.GetKey(KeyCode.LeftShift) && stamina > 0f && !isExhausted;
        if (isSprinting)
        {
            stamina -= Time.deltaTime * 10f;
            if (stamina <= 0f)
            {
                stamina = 0f;
                isExhausted = true;
            }
        }
        else
        {
            stamina += Time.deltaTime * 5f;
            stamina = Mathf.Clamp(stamina, 0f, 100f);
        }

        
    }

    [System.Obsolete]
    void FixedUpdate()
    {
        MovePlayer();
    }

    [System.Obsolete]
    private void MovePlayer()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        if (Input.GetKeyDown(KeyCode.Space) && Mathf.Abs(rb.velocity.y) < 0.001f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        Vector3 targetVelocity = moveDirection * currentSpeed;
        targetVelocity.y = rb.velocity.y;

        rb.velocity = targetVelocity;
    }
}

