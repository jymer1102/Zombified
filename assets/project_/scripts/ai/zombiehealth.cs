using UnityEngine;
using Project.Interfaces;

public class ZombieHealth : MonoBehaviour, IDamageable
{
    public enum ZombieTier { Regular, Strong, Boss }

    [Header("Tier Settings")]
    [Tooltip("Set this per-prefab: Regular zombie, Strong (bigger/tougher), or the level Boss.")]
    public ZombieTier tier = ZombieTier.Regular;

    [Header("Health Settings (set automatically based on Tier, or override manually)")]
    public float regularHealth = 100f;
    public float strongHealth = 220f;
    public float bossHealth = 600f;

    private float maxHealth;
    private float currentHealth;

    private bool isDead = false;
    private ZombieAI zombieAI;

    void Awake()
    {
        zombieAI = GetComponent<ZombieAI>();
    }

    void OnEnable()
    {
        // Reset state values whenever the object gets pulled from the pool
        maxHealth = GetHealthForTier(tier);
        currentHealth = maxHealth;
        isDead = false;
    }

    float GetHealthForTier(ZombieTier t)
    {
        switch (t)
        {
            case ZombieTier.Strong: return strongHealth;
            case ZombieTier.Boss: return bossHealth;
            default: return regularHealth;
        }
    }

    /// <summary>
    /// Implements the IDamageable interface to process incoming damage.
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} ({tier}) took {damageAmount} damage. Health remaining: {currentHealth}/{maxHealth}");

        // Interrupt their attack/chase path to play a stagger effect
        if (zombieAI != null && currentHealth > 0f)
        {
            zombieAI.TriggerFlinch();
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} ({tier}) has been eliminated!");

        // Update the level tracking system
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterKill();
        }

        // Trigger local 3D audio death sound if available
        if (AudioManager.Instance != null && AudioManager.Instance.zombieDeathClip != null) 
        {
            AudioManager.Instance.Play3DSFX(AudioManager.Instance.zombieDeathClip, transform.position);
        }

        // Return to pool instead of calling Destroy(gameObject)
        gameObject.SetActive(false);
    }

    public float GetHealthPercentage()
    {
        return maxHealth > 0 ? currentHealth / maxHealth : 0f;
    }

    /// <summary>
    /// Returns true if the given amount of incoming damage would kill this zombie.
    /// Used by PlayerCombat to decide whether the hitmarker should flash red (kill) or white (hit).
    /// </summary>
    public bool WouldDie(float incomingDamage)
    {
        return !isDead && (currentHealth - incomingDamage) <= 0f;
    }
}
