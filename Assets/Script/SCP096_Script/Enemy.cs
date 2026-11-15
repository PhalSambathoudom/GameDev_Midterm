using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerHealth target;      
    [SerializeField] private float detectionRange = 30f;

    [Header("Movement")]
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackDamage = 3f;
    [SerializeField] private float attackCooldown = 1.5f;

    private NavMeshAgent agent;
    private float nextAttackTime;
    private float stunnedUntil;
    private float nextPathTime;

    public bool IsStunned => Time.time < stunnedUntil;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;
        agent.stoppingDistance = attackRange * 0.8f;
        if (target == null) target = FindFirstObjectByType<PlayerHealth>();
    }

    void Update()
    {
        if (target == null) return;

        if (IsStunned || target.IsDead)
        {
            if (agent.enabled && !agent.isStopped) { agent.isStopped = true; agent.ResetPath(); }
            return;
        }

        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (dist > detectionRange)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        if (Time.time >= nextPathTime)
        {
            agent.SetDestination(target.transform.position);
            nextPathTime = Time.time + 0.2f;
        }

        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            target.TakeDamage(attackDamage);
            // TODO: play attack animation / sound
        }
    }

    public void Stun(float seconds)
    {
        stunnedUntil = Time.time + seconds;
        nextAttackTime = Mathf.Max(nextAttackTime, stunnedUntil); // can't hit the instant the stun ends
        // TODO: play stunned animation / sound
    }
}