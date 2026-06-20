using UnityEngine;
using Project.Interfaces;

public class GrenadeProjectile : MonoBehaviour
{
    [Header("Explosion Mechanics")]
    public float fuseDuration = 3.0f;
    public float blastRadius = 6.0f;
    public float explosionDamage = 150f;

    private float fuseTimer = 0f;
    private bool hasExploded = false;

    void Start()
    {
        fuseTimer = fuseDuration;
    }

    void Update()
    {
        fuseTimer -= Time.deltaTime;
        if (fuseTimer <= 0f && !hasExploded)
        {
            Explode();
        }
    }

    void Explode()
    {
        hasExploded = true;
        Debug.Log("GRENADE DETONATED: Calculating physics blast radius.");

        // Trigger dramatic camera screen shake upon explosion impact
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.TriggerShake(0.35f, 0.25f);
        }

        // Trigger global 3D spatial sound effect at this exact position
        // if (AudioManager.Instance != null) AudioManager.Instance.Play3DSFX(AudioManager.Instance.explosionClip, transform.position);

        // Find all physical objects caught within the spherical explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, blastRadius);

        foreach (Collider hit in hitColliders)
        {
            // Calculate if the object caught in the radius can take damage
            IDamageable damageableTarget = hit.GetComponent<IDamageable>();
            if (damageableTarget != null)
            {
                // Simple falloff calculation: Deal less damage if they are further from the center
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                float damagePercentage = 1f - (distance / blastRadius);
                float finalDamage = Mathf.Clamp(explosionDamage * damagePercentage, 0f, explosionDamage);

                damageableTarget.TakeDamage(finalDamage);
            }
        }

        // Remove the visual grenade projectile model from the scene world
        Destroy(gameObject);
    }

    // Optional: Draw the radius circle in the Unity Editor for visual layout debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}
