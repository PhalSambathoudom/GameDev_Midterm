using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform handSlot;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private int inventorySize = 1;
    [SerializeField] private float attackRadius = 0.5f;

    public PlayerHealth Health { get; private set; }

    private InputActionMap playerMap;
    private InputAction interactAction, attackAction;
    private WeaponPickup weapon;
    private float nextAttackTime;
    private IInteractable looking;

    void Awake()
    {
        Health = GetComponent<PlayerHealth>();
        playerMap = inputActions.FindActionMap("Player", true);
        interactAction = playerMap.FindAction("Interact", true);
        attackAction = playerMap.FindAction("Attack", true);
    }

    void OnEnable() => playerMap.Enable();
    void OnDisable() { looking = null; playerMap.Disable(); }

    void Update()
    {
        if (Health.IsDead) { looking = null; return; }

        looking = null;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                out RaycastHit hit, interactRange, ~0, QueryTriggerInteraction.Ignore))
            looking = hit.collider.GetComponentInParent<IInteractable>();

        if (looking != null && interactAction.WasPressedThisFrame())
            looking.Interact(this);

        if (attackAction.WasPressedThisFrame()) TryAttack();
    }

    public void PickUp(WeaponPickup newWeapon)
    {
        // inventory full -> swap: drop the current one where the new one was
        if (weapon != null && inventorySize <= 1)
        {
            weapon.OnDrop(newWeapon.transform.position);
            weapon = null;
        }
        weapon = newWeapon;
        weapon.OnEquip(handSlot);
    }

    private void TryAttack()
    {
        if (weapon == null || Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + weapon.cooldown;

        if (Physics.SphereCast(playerCamera.transform.position, attackRadius,
                playerCamera.transform.forward, out RaycastHit hit, weapon.range, ~0,
                QueryTriggerInteraction.Ignore))
        {
            var enemy = hit.collider.GetComponentInParent<Enemy>();
            if (enemy != null) enemy.Stun(weapon.stunDuration);
        }
        // TODO: play swing animation / sound here
    }

    void OnGUI()
    {
        if (Health.IsDead)
        {
            GUI.Label(new Rect(Screen.width / 2f - 150, Screen.height / 2f, 300, 30), "YOU DIED - reviving...");
            return;
        }
        if (looking != null && !string.IsNullOrEmpty(looking.Prompt))
            GUI.Label(new Rect(Screen.width / 2f - 150, Screen.height / 2f + 40, 300, 30), looking.Prompt);
    }
}