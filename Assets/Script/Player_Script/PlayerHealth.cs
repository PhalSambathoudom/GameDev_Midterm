using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHp = 15f;
    [SerializeField] private float autoHealRate = 1f;
    [SerializeField] private float healDelayAfterHit = 3f;

    [Header("Revive (from your story)")]
    [SerializeField] private float reviveDelay = 10f;
    [SerializeField] private int maxRevives = 3;
    [SerializeField] private Transform startPoint;

    private Player player;
    private float hp;
    private float lastHitTime = -999f;
    private float reviveEndTime;
    private bool isDead;
    private int revivesUsed;
    private Vector3 checkpointPos;
    private Quaternion checkpointRot;

    public bool HasFlag { get; private set; }
    public bool IsDead => isDead;
    public float ReviveTimeRemaining => isDead ? Mathf.Max(0f, reviveEndTime - Time.time) : 0f;
    public float Hp => hp;
    public float MaxHp => maxHp;

    void Awake()
    {
        player = GetComponent<Player>();
        if (player == null) Debug.LogError("PlayerHealth needs the Player script on the SAME object: " + name, this);
        hp = maxHp;
        if (startPoint == null)
        {
            // fall back to wherever the player starts in the scene
            var go = new GameObject("PlayerStartPoint");
            go.transform.SetPositionAndRotation(transform.position, transform.rotation);
            startPoint = go.transform;
        }
    }

    void Update()
    {
        if (isDead || !HasFlag || hp >= maxHp) return;
        if (Time.time - lastHitTime < healDelayAfterHit) return;
        hp = Mathf.Min(maxHp, hp + autoHealRate * Time.deltaTime);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        hp -= amount;
        lastHitTime = Time.time;
        if (hp <= 0f) { hp = 0f; StartCoroutine(DieRoutine()); }
    }

    public void ClaimFlag(Vector3 flagPosition, Quaternion facing)
    {
        HasFlag = true;
        checkpointPos = flagPosition + Vector3.up * 1f;
        checkpointRot = facing;
    }

    private IEnumerator DieRoutine()
    {
        isDead = true;
        reviveEndTime = Time.time + reviveDelay;
        if (player != null) player.enabled = false;

        yield return new WaitForSeconds(reviveDelay);

        if (HasFlag && revivesUsed < maxRevives)
        {
            revivesUsed++;
            player.TeleportTo(checkpointPos, checkpointRot);
        }
        else
        {
            // out of revives -> back to start, progress reset
            HasFlag = false;
            revivesUsed = 0;
            if (player != null) player.TeleportTo(startPoint.position, startPoint.rotation);
        }

        hp = maxHp;
        isDead = false;
        if (player != null) player.enabled = true;
    }
}