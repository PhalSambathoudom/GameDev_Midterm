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

    private Player player;
    private InputActionMap playerMap;
    private InputAction interactAction, attackAction;
    private WeaponPickup weapon;
    private float nextAttackTime;
    private IInteractable looking;
    private GUIStyle hudLabelStyle;
    private GUIStyle hudStatusStyle;
    private GUIStyle promptStyle;

    void Awake()
    {
        Health = GetComponent<PlayerHealth>();
        player = GetComponent<Player>();
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
        CreateHudStyles();
        float scale = Mathf.Clamp(Screen.height / 900f, 0.7f, 1.3f);

        if (Health.IsDead)
        {
            Rect deadPanel = new Rect(Screen.width * 0.5f - 180f * scale,
                Screen.height * 0.5f - 35f * scale, 360f * scale, 70f * scale);
            DrawPanel(deadPanel);
            int secondsRemaining = Mathf.CeilToInt(Health.ReviveTimeRemaining);
            GUI.Label(deadPanel, $"YOU DIED - REVIVING IN {secondsRemaining}s", hudStatusStyle);
            return;
        }

        DrawHud(scale);

        if (looking != null && !string.IsNullOrEmpty(looking.Prompt))
        {
            Rect promptRect = new Rect(Screen.width * 0.5f - 190f * scale,
                Screen.height * 0.5f + 36f * scale, 380f * scale, 38f * scale);
            DrawPanel(promptRect);
            GUI.Label(promptRect, looking.Prompt, promptStyle);
        }
    }

    private void DrawHud(float scale)
    {
        float panelWidth = 270f * scale;
        float panelHeight = 126f * scale;
        Rect panel = new Rect(20f * scale, Screen.height - panelHeight - 20f * scale,
            panelWidth, panelHeight);
        DrawPanel(panel);

        float padding = 14f * scale;
        float labelHeight = 20f * scale;
        float barHeight = 12f * scale;
        float barWidth = panel.width - padding * 2f;
        float y = panel.y + 10f * scale;

        GUI.Label(new Rect(panel.x + padding, y, barWidth, labelHeight),
            $"HEALTH  {Mathf.CeilToInt(Health.Hp)} / {Mathf.CeilToInt(Health.MaxHp)}", hudLabelStyle);
        y += labelHeight;
        DrawStatBar(new Rect(panel.x + padding, y, barWidth, barHeight),
            Health.Hp, Health.MaxHp, new Color(0.78f, 0.16f, 0.19f));
        y += barHeight + 8f * scale;

        GUI.Label(new Rect(panel.x + padding, y, barWidth, labelHeight),
            $"STAMINA  {Mathf.CeilToInt(player.Stamina)} / {Mathf.CeilToInt(player.MaxStamina)}", hudLabelStyle);
        y += labelHeight;
        DrawStatBar(new Rect(panel.x + padding, y, barWidth, barHeight),
            player.Stamina, player.MaxStamina, new Color(0.2f, 0.65f, 0.84f));

        Rect statusRect = new Rect(panel.x + padding, panel.y + 88f * scale,
            barWidth, 24f * scale);
        string weaponStatus = weapon != null ? weapon.weaponName : "No weapon";
        string checkpointStatus = Health.HasFlag ? "Checkpoint secured" : "Find the flag";
        GUI.Label(statusRect, $"{weaponStatus}  |  {checkpointStatus}", hudLabelStyle);
    }

    private void DrawStatBar(Rect rect, float value, float maximum, Color fillColor)
    {
        Color previousColor = GUI.color;
        float fraction = maximum > 0f ? Mathf.Clamp01(value / maximum) : 0f;
        GUI.color = new Color(0.12f, 0.13f, 0.16f, 0.95f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        rect.width *= fraction;
        GUI.color = fillColor;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = previousColor;
    }

    private void DrawPanel(Rect rect)
    {
        Color previousColor = GUI.color;
        GUI.color = new Color(0.04f, 0.05f, 0.07f, 0.82f);
        GUI.Box(rect, GUIContent.none);
        GUI.color = previousColor;
    }

    private void CreateHudStyles()
    {
        if (hudLabelStyle == null)
        {
            hudLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            hudStatusStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        float scale = Mathf.Clamp(Screen.height / 900f, 0.7f, 1.3f);
        hudLabelStyle.fontSize = Mathf.RoundToInt(14f * scale);
        hudStatusStyle.fontSize = Mathf.RoundToInt(20f * scale);
        promptStyle.fontSize = Mathf.RoundToInt(15f * scale);
    }
}