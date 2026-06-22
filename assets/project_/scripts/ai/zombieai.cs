using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public enum AIState { Idle, Patrol, Chase, Attack, Flinch }

    [Header("State Machine")]
    public AIState currentState = AIState.Idle;

    [Header("Detection Settings")]
    public float lookRadius = 15f;
    [Tooltip("Detection radius used when the player is crouching in darkness (true stealth).")]
    public float stealthLookRadius = 4f;
    [Tooltip("Detection radius used when crouching but still lit up by a nearby light source - partial stealth.")]
    public float litCrouchLookRadius = 9f;
    public float attackRadius = 2f;
    public float patrolRadius = 10f;

    [Header("Combat Configuration")]
    public int attackDamage = 15;
    public float attackRate = 1.5f;
    private float attackTimer = 0f;

    [Header("Flinch Mechanics")]
    public float flinchDuration = 0.4f;
    [Tooltip("Brief window after a flinch ends where new hits can't immediately re-trigger flinch. Stops stunlock.")]
    public float flinchImmunityWindow = 0.25f;
    private float flinchTimer = 0f;
    private float flinchImmunityTimer = 0f;

    private NavMeshAgent agent;
    private Transform playerTarget;
    private Vector3 patrolTarget;
    private PlayerMovement playerMovement;

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
        flinchImmunityTimer = 0f;
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
        if (flinchImmunityTimer > 0) flinchImmunityTimer -= Time.deltaTime;

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
            playerMovement = player.GetComponent<PlayerMovement>();
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
        float effectiveLookRadius = GetEffectiveLookRadius();

        // Target is close enough to claw/bite
        if (distanceToPlayer <= attackRadius)
        {
            currentState = AIState.Attack;
        }
        // Lost track of player (using a slightly larger radius than detection so they don't flicker in/out)
        else if (distanceToPlayer > effectiveLookRadius * 1.5f)
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

        if (AudioManager.Instance != null && AudioManager.Instance.zombieAttackClip != null)
        {
            AudioManager.Instance.Play3DSFX(AudioManager.Instance.zombieAttackClip, transform.position);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(attackDamage);
        }
    }

    /// <summary>
    /// Forces the AI to break out of its current path and stumble backward momentarily.
    /// Called directly by Weapon hit systems. Ignored if still in the post-flinch immunity window,
    /// which stops rapid fire from permanently stunlocking the zombie.
    /// </summary>
    public void TriggerFlinch()
    {
        if (currentState == AIState.Flinch) return;
        if (flinchImmunityTimer > 0f) return;

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
            flinchImmunityTimer = flinchImmunityWindow;
            currentState = AIState.Chase;
        }
    }

    /// <summary>
    /// Returns the detection range that should currently apply, using three tiers:
    /// - Standing or moving normally: full lookRadius, light doesn't matter
    /// - Crouching in good light: litCrouchLookRadius (partial stealth - you're trying, but visible)
    /// - Crouching in near-total darkness: stealthLookRadius (true stealth - the README's promise)
    /// </summary>
    float GetEffectiveLookRadius()
    {
        if (playerMovement == null || !playerMovement.IsCrouching)
        {
            return lookRadius;
        }

        // Crouching - now check how lit up the player actually is using real light detection
        float lightLevel = AmbientLightDetector.Instance != null ? AmbientLightDetector.Instance.GetLightLevel() : 0f;

        // Blend between full stealth and partial stealth based on how lit the player is,
        // rather than a hard on/off switch - feels more natural than a binary "in light / not in light"
        return Mathf.Lerp(stealthLookRadius, litCrouchLookRadius, lightLevel);
    }

    bool EvaluatePlayerDetection()
    {
        if (playerTarget == null) return false;

        float distance = Vector3.Distance(transform.position, playerTarget.position);
        float effectiveLookRadius = GetEffectiveLookRadius();

        if (distance <= effectiveLookRadius)
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
