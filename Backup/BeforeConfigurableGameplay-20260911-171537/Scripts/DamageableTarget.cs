using UnityEngine;

// Optional health component for objects that should receive melee damage.
public class DamageableTarget : MonoBehaviour, IDamageable
{
    public float health = 100f;
    public void TakeDamage(float damage)
    {
        health -= Mathf.Max(0f, damage);
        if (health <= 0f) Destroy(gameObject);
    }
}
