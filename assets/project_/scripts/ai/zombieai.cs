using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour
{
    public enum ZombieType { Easy, Knife, Strong, Boss2026 }
    
    [Header("Zombie Profile")]
    public ZombieType type = ZombieType.Easy;
    public float detectionRadius = 15f;
    public float fieldOfView = 120f; // Specified 120 degree vision cone

    [Header("References")]
    private Transform playerTransform;
    private PlayerMovement playerMovement;
    private HeadlampController headlamp;
    private NavMeshAgent agent;

    private bool isAlerted = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Dynamically find the player setup in the scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerMovement = player.GetComponent<PlayerMovement>();
            headlamp = player.GetComponent<HeadlampController>();
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        if (isAlerted)
        {
            // Chase down target
            agent.SetDestination(playerTransform.position);
        }
        else
        {
            CheckForPlayer();
        }
    }

    void CheckForPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // If player is inside potential tracking distance
        if (distanceToPlayer <= detectionRadius)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            // 1. Check Vision Cone (120 degrees total means 60 degrees left or right)
            if (angleToPlayer <= fieldOfView / 2f)
            {
                // Raycast to ensure no walls are in the way
                if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out RaycastHit hit, detectionRadius))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        AlertZombie();
                        return;
                    }
                }
            }

            // 2. Tactical Stealth Rule Check (Sneaking up from behind)
            bool isSneaking = playerMovement != null && playerMovement.IsStealthing();
            bool isLightOn = headlamp != null && headlamp.IsHeadlampActive();

            // If the player is NOT sneaking, or if they have their bright headlamp turned on, the zombie hears/sees them automatically
            if (!isSneaking || isLightOn)
            {
                AlertZombie();
            }
        }
    }

    public void AlertZombie()
    {
        isAlerted = true;
        Debug.Log($"{gameObject.name} (Type: {type}) has targeted the player!");
    }
}
