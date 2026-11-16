using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerHealth target;
    [SerializeField] private float detectionRange = 30f;

    [Header("SCP-096 Trigger")]
    [SerializeField] private float rageDelay = 10f;
    [SerializeField, Range(1f, 30f)] private float viewAngle = 15f;
    [SerializeField] private Vector3 viewTargetOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Audio")]
    [SerializeField] private AudioClip triggerSound;
    [SerializeField] private AudioClip cryingSound;
    [SerializeField] private AudioClip transitionScreamSound;
    [SerializeField] private AudioClip chasingSound;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip stunnedSound;
    [SerializeField, Range(0f, 1f)] private float chasingSoundVolume = 0.75f;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float navMeshSearchRadius = 3f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackDamage = 3f;
    [SerializeField] private float attackCooldown = 1.5f;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private AudioSource chasingAudioSource;
    private PlayerInteract playerInteract;
    private GameManager gameManager;
    private Renderer viewRenderer;
    private AudioClip activeLoop;
    private AudioSource activeLoopSource;
    private float nextAttackTime;
    private float stunnedUntil;
    private float nextPathTime;
    private float passiveTimer;
    private bool hasBeenSeen;
    private bool isEnraged;
    private bool warnedAboutNavMesh;
    private bool warnedAboutDestination;

    public bool IsStunned => Time.time < stunnedUntil;
    [SerializeField, Range(0f, 1f)] private float screamVolume = 0.35f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;
        agent.stoppingDistance = attackRange * 0.8f;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 6f;
        audioSource.maxDistance = detectionRange;
        chasingAudioSource = gameObject.AddComponent<AudioSource>();
        chasingAudioSource.playOnAwake = false;
        chasingAudioSource.spatialBlend = 1f;
        chasingAudioSource.rolloffMode = AudioRolloffMode.Linear;
        chasingAudioSource.minDistance = 6f;
        chasingAudioSource.maxDistance = detectionRange;
        chasingAudioSource.volume = chasingSoundVolume;
        viewRenderer = GetComponentInChildren<Renderer>();
        if (target == null) target = FindFirstObjectByType<PlayerHealth>();
        if (target != null) playerInteract = target.GetComponent<PlayerInteract>();
        gameManager = FindFirstObjectByType<GameManager>();
        TryEnsureAgentOnNavMesh();
        StopAgent();

        bool hasEnabledRenderer = false;
        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.enabled && renderer.gameObject.activeInHierarchy)
            {
                hasEnabledRenderer = true;
                break;
            }
        }

        if (!hasEnabledRenderer)
            Debug.LogWarning(
                "SCP096 has no active renderer. The model child GameObject is probably disabled in the Hierarchy.",
                this);

        foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            smr.updateWhenOffscreen = true;

        Collider playerCollider = target != null ? target.GetComponent<Collider>() : null;
        if (playerCollider != null)
        {
            foreach (var c in GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(playerCollider, c, true);
        }
    }

    void Update()
    {

        if (target == null || Time.timeScale <= 0f) return;

        if (target.IsDead)
        {
            StopAgent();
            StopLoop();
            return;
        }

        passiveTimer += Time.deltaTime;

        if (!hasBeenSeen && IsFaceViewed())
        {
            hasBeenSeen = true;
            if (gameManager != null)
                gameManager.PlaySCP096SeenSequence(triggerSound);
            else
                PlaySound(triggerSound);
        }

        if (IsStunned)
        {
            StopAgent();
            StopLoop();
            return;
        }

        if (!isEnraged && passiveTimer < rageDelay)
        {
            PlayLoop(cryingSound);
            StopAgent();
            return;
        }

        if (!TryEnsureAgentOnNavMesh()) return;

        if (!isEnraged)
        {
            isEnraged = true;
            Debug.Log($"[{Time.timeSinceLevelLoad:F2}s] SCP096 enraged", this);
            PlayLoop(chasingSound);
            PlaySound(transitionScreamSound, screamVolume);
        }
        else
        {
            PlayLoop(chasingSound);
        }

        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (!agent.enabled || !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = false;
        if (Time.time >= nextPathTime)
        {
            Vector3 destination = target.transform.position;
            if (NavMesh.SamplePosition(destination, out NavMeshHit targetHit, navMeshSearchRadius, agent.areaMask))
                destination = targetHit.position;

            if (!agent.SetDestination(destination) && !warnedAboutDestination)
            {
                Debug.LogWarning("SCP-096 could not calculate a path to the player. Check the baked NavMesh.", this);
                warnedAboutDestination = true;
            }
            nextPathTime = Time.time + 0.2f;
        }

        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            target.TakeDamage(attackDamage);
            PlaySound(attackSound);
        }
    }

    private bool IsFaceViewed()
    {
        Camera playerCamera = playerInteract != null ? playerInteract.PlayerCamera : null;
        if (playerCamera == null) return false;

        Vector3 viewTarget = viewRenderer != null
            ? viewRenderer.bounds.center
            : transform.TransformPoint(viewTargetOffset);
        Vector3 toTarget = viewTarget - playerCamera.transform.position;
        float distance = toTarget.magnitude;
        if (distance <= 0f || distance > detectionRange) return false;

        Vector3 direction = toTarget / distance;
        if (Vector3.Angle(playerCamera.transform.forward, direction) > viewAngle) return false;

        if (Physics.Raycast(playerCamera.transform.position, direction, out RaycastHit hit, distance,
                ~0, QueryTriggerInteraction.Ignore) &&
            hit.transform != transform && !hit.transform.IsChildOf(transform))
            return false;

        return true;
    }

    private bool TryEnsureAgentOnNavMesh()
    {
        if (!agent.enabled) return false;
        if (agent.isOnNavMesh) return true;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, navMeshSearchRadius, agent.areaMask) &&
            agent.Warp(hit.position) && agent.isOnNavMesh)
            return true;

        if (!warnedAboutNavMesh)
        {
            Debug.LogWarning(
                "SCP-096 is not on a baked NavMesh. Bake the scene NavMesh or move SCP-096 closer to a walkable area.",
                this);
            warnedAboutNavMesh = true;
        }

        return false;
    }

    private void StopAgent()
    {
        if (agent.enabled && agent.isOnNavMesh && !agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void PlayLoop(AudioClip clip)
    {
        if (clip == null)
        {
            StopLoop();
            return;
        }

        AudioSource loopSource = clip == chasingSound ? chasingAudioSource : audioSource;
        if (activeLoop == clip && activeLoopSource == loopSource && loopSource.isPlaying) return;

        if (activeLoopSource != null)
            activeLoopSource.Stop();
        loopSource.clip = clip;
        loopSource.loop = true;
        activeLoop = clip;
        activeLoopSource = loopSource;
        loopSource.Play();
    }

    private void StopLoop()
    {
        if (activeLoop == null) return;

        if (activeLoopSource != null)
        {
            activeLoopSource.Stop();
            activeLoopSource.clip = null;
            activeLoopSource.loop = false;
        }
        activeLoop = null;
        activeLoopSource = null;
    }

    private void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null) audioSource.PlayOneShot(clip, volume);
    }

    public void Stun(float seconds)
    {
        stunnedUntil = Time.time + seconds;
        nextAttackTime = Mathf.Max(nextAttackTime, stunnedUntil);
        StopLoop();
        PlaySound(stunnedSound);
    }
}
