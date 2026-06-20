using UnityEngine;
using Project.Interfaces;

public class ZombieHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Implements the IDamageable interface to process incoming damage.
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health remaining: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log($"{gameObject.name} has been eliminated!");

        // Update the level tracking system
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterKill();
        }

        // Trigger local 3D audio death sound if available
        // if (AudioManager.Instance != null) AudioManager.Instance.Play3DSFX(AudioManager.Instance.zombieDeathClip, transform.position);

        Destroy(gameObject);
    }
}
