using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public enum AIState { Idle, Patrol, Chase, Attack, Flinch }

    [Header("State Machine")]
    public AIState currentState = AIState.Idle;

    [Header("Detection Settings")]
    public float lookRadius = 15f;
    public float attackRadius = 2f;
    public float patrolRadius = 10f;

    [Header("Combat Configuration")]
    public int attackDamage = 15;
    public float attackRate = 1.5f;
    private float attackTimer = 0f;

    [Header("Flinch Mechanics")]
    public float flinchDuration = 0.4f;
    private float flinchTimer = 0f;

    private NavMeshAgent agent;
    private Transform playerTarget;
    private Vector3 patrolTarget;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void OnEnable()
    {
        // Reset state machine cleanly whenever pulled from the Object Pool
        currentState = AIState.Idle;
        attackTimer = 0f;
        flinchTimer = 0f;
        FindPlayerTarget();
        ChooseNewPatrolPoint();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
        {
            agent.isStopped = true;
            return;
        }

        // Handle cooldown timers
        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        // Execute behaviors based on current state
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdleState();
                break;
            case AIState.Patrol:
                HandlePatrolState();
                break;
            case AIState.Chase:
                HandleChaseState();
                break;
            case AIState.Attack:
                HandleAttackState();
                break;
            case AIState.Flinch:
                HandleFlinchState();
                break;
        }
    }

    void FindPlayerTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }
    }

    void HandleIdleState()
    {
        if (agent.hasRemainingDistance == false) agent.isStopped = true;

        if (EvaluatePlayerDetection()) return;

        // Transition to patrolling out of boredom
        if (Random.value < 0.01f)
        {
            ChooseNewPatrolPoint();
            currentState = AIState.Patrol;
        }
    }

    void HandlePatrolState()
    {
        agent.isStopped = false;

        if (EvaluatePlayerDetection()) return;

        // Check if arrived at patrol point
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = AIState.Idle;
        }
    }

    void HandleChaseState()
    {
        if (playerTarget == null) { currentState = AIState.Idle; return; }

        agent.isStopped = false;
        agent.SetDestination(playerTarget.position);

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        
        // Target is close enough to claw/bite
        if (distanceToPlayer <= attackRadius)
        {
            currentState = AIState.Attack;
        }
        // Lost track of player
        else if (distanceToPlayer > lookRadius)
        {
            ChooseNewPatrolPoint();
            currentState = AIState.Patrol;
        }
    }

    void HandleAttackState()
    {
        if (playerTarget == null) { currentState = AIState.Idle; return; }

        agent.isStopped = true;
        // Keep facing the player during execution
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > attackRadius)
        {
            currentState = AIState.Chase;
            return;
        }

        if (attackTimer <= 0f)
        {
            ExecuteBiteAttack();
        }
    }

    void ExecuteBiteAttack()
    {
        attackTimer = attackRate;
        Debug.Log($"{gameObject.name} slashed the player!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(attackDamage);
        }
    }

    /// <summary>
    /// Forces the AI to break out of its current path and stumble backward momentarily.
    /// Called directly by Weapon hit systems.
    /// </summary>
    public void TriggerFlinch()
    {
        if (currentState == AIState.Flinch) return;

        currentState = AIState.Flinch;
        flinchTimer = flinchDuration;
        agent.isStopped = true;
        Debug.Log($"{gameObject.name} staggered from bullet velocity impact.");
    }

    void HandleFlinchState()
    {
        flinchTimer -= Time.deltaTime;
        if (flinchTimer <= 0f)
        {
            // Re-evaluate what to do after recovering from shock
            currentState = AIState.Chase;
        }
    }

    bool EvaluatePlayerDetection()
    {
        if (playerTarget == null) return false;

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        if (distance <= lookRadius)
        {
            currentState = AIState.Chase;
            return true;
        }
        return false;
    }

    void ChooseNewPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        NavMeshHit navHit;
        
        // Find a valid spot nearby on Unity's baked tracking navigation layout
        if (NavMesh.SamplePosition(randomDirection, out navHit, patrolRadius, -1))
        {
            patrolTarget = navHit.position;
            agent.SetDestination(patrolTarget);
        }
    }
}
