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
    [SerializeField] private int maxRevives = 5;
    [SerializeField] private Transform startPoint;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip reviveCountdownSound;
    [SerializeField] private AudioClip respawnSound;

    private Player player;
    private AudioSource audioSource;
    private AudioSource countdownSource;
    private float hp;
    private float lastHitTime = -999f;
    private float reviveEndTime;
    private bool isDead;
    private int revivesUsed;
    private Vector3 checkpointPos;
    private Quaternion checkpointRot;

    public bool HasFlag { get; private set; }
    public bool IsDead => isDead;
    public float ReviveTimeRemaining => isDead ? Mathf.Max(0f, reviveEndTime - Time.realtimeSinceStartup) : 0f;
    public float Hp => hp;
    public float MaxHp => maxHp;
    [SerializeField] private float fallLimit = 15f;
    [SerializeField] private float respawnProtection = 3f;
    private float invulnerableUntil;

    void Awake()
    {
        player = GetComponent<Player>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        if (player == null) Debug.LogError("PlayerHealth needs the Player script on the SAME object: " + name, this);
        hp = maxHp;
        if (startPoint == null)
        {
            // fall back to wherever the player starts in the scene
            var go = new GameObject("PlayerStartPoint");
            go.transform.SetPositionAndRotation(transform.position, transform.rotation);
            startPoint = go.transform;
        }
        countdownSource = gameObject.AddComponent<AudioSource>();
countdownSource.playOnAwake = false;
countdownSource.spatialBlend = 0f;
    }

    void Update()
    {
        if (!isDead && player != null && startPoint != null &&
            transform.position.y < startPoint.position.y - fallLimit)
        {
            Debug.LogWarning("Player fell out of the maze, check the floor collider", this);
            player.TeleportTo(startPoint.position + Vector3.up * 0.5f, startPoint.rotation);
            return;
        }
        if (isDead || !HasFlag || hp >= maxHp) return;
        if (Time.time - lastHitTime < healDelayAfterHit) return;
        hp = Mathf.Min(maxHp, hp + autoHealRate * Time.deltaTime);
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f || Time.time < invulnerableUntil) return;
        hp -= amount;
        lastHitTime = Time.time;
        if (hp < 0f) hp = 0f;
        Debug.Log($"[{Time.timeSinceLevelLoad:F2}s] Player took {amount} damage. HP left: {hp}/{maxHp}.", this);

        if (hp <= 0f)
        {
            PlayClip(deathSound);
            StartCoroutine(DieRoutine());
        }
    }

    public void ClaimFlag(Vector3 flagPosition, Quaternion facing)
    {
        HasFlag = true;
        checkpointPos = flagPosition + Vector3.up * 0.1f;
        checkpointRot = facing;
    }

    private IEnumerator DieRoutine()
{
    isDead = true;
    Debug.Log($"[{Time.timeSinceLevelLoad:F2}s] Player death routine started.", this);
    reviveEndTime = Time.realtimeSinceStartup + reviveDelay;
    if (player != null) player.SetFrozen(true);

    try
    {
        float countdownLength = reviveCountdownSound != null ? reviveCountdownSound.length : 0f;
        float countdownStart = Mathf.Max(0f, reviveDelay - countdownLength);

        if (countdownStart > 0f) yield return new WaitForSecondsRealtime(countdownStart);

        if (reviveCountdownSound != null)
        {
            countdownSource.clip = reviveCountdownSound;
            countdownSource.loop = false;
            countdownSource.Play();
        }

        float remaining = reviveDelay - countdownStart;
        if (remaining > 0f) yield return new WaitForSecondsRealtime(remaining);

        audioSource.Stop();
        countdownSource.Stop();

        if (HasFlag && revivesUsed < maxRevives)
        {
            revivesUsed++;
            if (player != null) player.TeleportTo(checkpointPos, checkpointRot);
        }
        else
        {
            // out of revives -> back to start, progress reset
            HasFlag = false;
            revivesUsed = 0;
            if (player != null && startPoint != null)
                player.TeleportTo(startPoint.position, startPoint.rotation);
        }

        hp = maxHp;
        invulnerableUntil = Time.time + respawnProtection;
        PlayClip(respawnSound);
    }
    finally
    {
        isDead = false;
        if (player != null) player.SetFrozen(false);
    }
}
    private void PlayClip(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = false;
        audioSource.Play();
    }
}