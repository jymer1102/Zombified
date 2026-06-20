namespace Project.Interfaces
{
    public interface IDamageable
    {
        /// <summary>
        /// Forces the object to take a specific amount of damage points.
        /// </summary>
        void TakeDamage(float damageAmount);
    }
}
